using System.Collections.Generic;

namespace GameItemEditor.Core.Models
{
    public class ArmorProperties
    {
        public int Defense { get; set; }
        public string Slot { get; set; }
        public int ResistFire { get; set; }
        public int Durability { get; set; } = 100;
        public Dictionary<string, object> Custom { get; set; } = new();
    }
}
