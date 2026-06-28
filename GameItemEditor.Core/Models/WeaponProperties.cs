using System.Collections.Generic;

namespace GameItemEditor.Core.Models
{
    public class WeaponProperties
    {
        public int Damage {  get; set; }
        public string DamageType { get; set; }
        public string CritChance { get; set; }
        public int Durability { get; set; } = 100;
        public Dictionary<string, object> Custom { get; set; } = new();
    }
}
