using GameItemEditor.Api.Data;
using GameItemEditor.Api.Dto;
using GameItemEditor.Api.Helpers;
using GameItemEditor.Core.Enums;
using GameItemEditor.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameItemEditor.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(AppDbContext context, ILogger<ItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemResponseDto>>> GetItems(
            [FromQuery] string? search = null,
            [FromQuery] ItemType? type = null,
            [FromQuery] ItemRarity? rarity = null)
        {
            try
            {
                var query = _context.GameItems.AsQueryable();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(item => item.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                if (type.HasValue)
                {
                    query = query.Where(item => item.Type == type.Value);
                }

                if (rarity.HasValue)
                {
                    query = query.Where(item => item.Rarity == rarity.Value);
                }

                query = query.OrderByDescending(item => item.CreatedAt);

                var items = await query.ToListAsync();

                var response = items.Select(ItemMapper.MapToResponseDto).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка предметов");
                return StatusCode(500, "Внутреняя ошибка сервера");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemResponseDto>> GetItem(Guid id)
        {
            try
            {
                var item = await _context.GameItems.FindAsync(id);

                if (item == null)
                {
                    return NotFound($"Предмет с ID {id} не найден");
                }

                return Ok(ItemMapper.MapToResponseDto(item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении предмета с ID {id}");
                return StatusCode(500, "Внутреняя ошибка сервера");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ItemResponseDto>> CreateItem([FromBody] CreateItemDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var item = ItemMapper.MapToGameItem(dto);

                _context.GameItems.Add(item);

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetItem), new { id = item.Id }, ItemMapper.MapToResponseDto(item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при создании предмета");
                return StatusCode(500, "Внутреняя ошибка сервера");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateItemDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingItem = await _context.GameItems.FindAsync(id);

                if (existingItem == null)
                {
                    return NotFound($"Предмет с ID {id} не найден");
                }

                var updateitem = ItemMapper.MapToGameItem(dto, existingItem);

                _context.Entry(updateitem).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении предмета с ID {id}");
                return StatusCode(500, "Внутреняя ошибка сервера");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            try
            {
                var item = await _context.GameItems.FindAsync(id);

                if (item == null)
                {
                    return NotFound($"Предмет с ID {id} не найден");
                }

                _context.GameItems.Remove(item);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении предмета с ID {id}");
                return StatusCode(500, "Внутреняя ошибка сервера");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchItem(Guid id, [FromBody] PatchItemDto dto)
        {
            try
            {
                var existingItem = await _context.GameItems.FindAsync(id);

                if (existingItem == null)
                {
                    return NotFound($"Предмет с ID {id} не найден");
                }

                var updateItem = ItemMapper.MapToGameItem(dto, existingItem);

                _context.GameItems.Entry(updateItem).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(ItemMapper.MapToResponseDto(updateItem));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при частичном обновлении предмета с ID {id}");
                return StatusCode(500, "Внутреняя ошибка сервера");
            }
        }
    }
}
