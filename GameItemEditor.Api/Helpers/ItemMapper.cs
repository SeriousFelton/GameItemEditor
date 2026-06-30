using System.Text.Json;
using GameItemEditor.Api.Configurations;
using GameItemEditor.Api.Dto;
using GameItemEditor.Core.Models;

namespace GameItemEditor.Api.Helpers
{

    public static class ItemMapper
    {
        private static readonly JsonSerializerOptions JsonOptions = GameItemEditor.Api.Configurations.JsonOptions.Default;

        public static GameItem MapToGameItem(CreateItemDto dto)
        {
            return new GameItem
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Type = dto.Type,
                Rarity = dto.Rarity,
                BasePrice = dto.BasePrice,
                Weight = dto.Weight,
                PropertiesJson = JsonSerializer.Serialize(dto.Properties, JsonOptions),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }

        public static GameItem MapToGameItem(UpdateItemDto dto, GameItem existingItem)
        {
            existingItem.Name = dto.Name;
            existingItem.Type = dto.Type;
            existingItem.Rarity = dto.Rarity;
            existingItem.BasePrice = dto.BasePrice;
            existingItem.Weight = dto.Weight;
            existingItem.PropertiesJson = JsonSerializer.Serialize(dto.Properties, JsonOptions);
            existingItem.UpdatedAt = DateTime.UtcNow;
            return existingItem;
        }

        public static ItemResponseDto MapToResponseDto(GameItem gameItem)
        {
            var properties = JsonSerializer.Deserialize<object>(gameItem.PropertiesJson, JsonOptions)
                             ?? new object();

            return new ItemResponseDto
            {
                Id = gameItem.Id,
                Name = gameItem.Name,
                Type = gameItem.Type,
                Rarity = gameItem.Rarity,
                BasePrice = gameItem.BasePrice,
                Weight = gameItem.Weight,
                Properties = properties,
                CreatedAt = gameItem.CreatedAt,
                UpdatedAt = gameItem.UpdatedAt
            };
        }

        public static GameItem MapToGameItem(PatchItemDto dto, GameItem existingItem)
        {
            if (dto.Name != null)
                existingItem.Name = dto.Name;

            if (dto.Type.HasValue)
                existingItem.Type = dto.Type.Value;

            if (dto.Rarity.HasValue)
                existingItem.Rarity = dto.Rarity.Value;

            if (dto.BasePrice.HasValue)
                existingItem.BasePrice = dto.BasePrice.Value;

            if (dto.Weight.HasValue)
                existingItem.Weight = dto.Weight.Value;

            if (dto.Properties != null)
                existingItem.PropertiesJson = JsonSerializer.Serialize(dto.Properties, JsonOptions);

            existingItem.UpdatedAt = DateTime.UtcNow;

            return existingItem;
        }
    }
}