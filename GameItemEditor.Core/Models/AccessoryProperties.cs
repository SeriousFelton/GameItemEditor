using System.Collections.Generic;

namespace GameItemEditor.Core.Models
{
    public class AccessoryProperties
    {
        public string StatBoost { get; set; }
        public int BoostValue { get; set; }
        public string Slot { get; set; }
        public Dictionary<string, object> Custom { get; set; } = new();
    }
}
