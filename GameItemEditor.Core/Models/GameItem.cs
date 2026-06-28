using System;
using GameItemEditor.Core.Enums;

namespace GameItemEditor.Core.Models
{
    public class GameItem
    {
        public Guid Id {  get; set; }
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public decimal BasePrice { get; set; }
        public double Weight { get; set; }

        public string PropertiesJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
