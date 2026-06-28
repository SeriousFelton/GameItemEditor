using System.ComponentModel.DataAnnotations;
using GameItemEditor.Core.Enums;

namespace GameItemEditor.Api.Dto
{
    public class CreateItemDto
    {
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 200 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Тип предмета обязателен")]
        public ItemType Type { get; set; }
        
        [Required(ErrorMessage = "Редкость обязательна")]
        public ItemRarity Rarity { get; set; }

        [Range(0, 1000000, ErrorMessage = "Цена должна быть от 0 до 1 000 000")]
        public decimal BasePrice { get; set; }

        [Range(0, 1000, ErrorMessage = "Вес должен быть от 0 до 1000")]
        public double Weight { get; set; }

        [Required(ErrorMessage = "Свойства предмета обязательны")]
        public object Properties { get; set; } = new();
    }
}
