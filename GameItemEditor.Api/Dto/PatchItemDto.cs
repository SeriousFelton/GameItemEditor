using System.ComponentModel.DataAnnotations;
using GameItemEditor.Core.Enums;

namespace GameItemEditor.Api.Dto
{

    public class PatchItemDto
    {
        public string? Name { get; set; }

        public ItemType? Type { get; set; }

        public ItemRarity? Rarity { get; set; }

        [Range(0, 1000000)]
        public decimal? BasePrice { get; set; }

        [Range(0, 1000)]
        public double? Weight { get; set; }

        public object? Properties { get; set; }
    }
}