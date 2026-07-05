using GameItemEditor.Core.Enums;

namespace GameItemEditor.Wpf.Models.Dto
{
    public class PatchItemDto
    {
        public string? Name { get; set; }
        public ItemType? Type { get; set; }
        public ItemRarity? Rarity { get; set; }
        public decimal? BasePrice { get; set; }
        public double? Weight { get; set; }
        public object? Properties { get; set; }
    }
}
