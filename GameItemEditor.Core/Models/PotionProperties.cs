using System.Collections.Generic;

namespace GameItemEditor.Core.Models
{
    public class PotionProperties
    {
        public string Effect { get; set; }
        public int DurationSeconds { get; set; }
        public int Magnitude { get; set; }
        public bool Stackable { get; set; }
        public Dictionary<string, object> Custom { get; set; } = new();
    }
}
