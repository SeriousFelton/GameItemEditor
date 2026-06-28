using GameItemEditor.Core.Enums;

namespace GameItemEditor.Api.Dto
{
    public class ItemResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public decimal BasePrice { get; set; }
        public double Weight { get; set; }
        public object Properties { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
