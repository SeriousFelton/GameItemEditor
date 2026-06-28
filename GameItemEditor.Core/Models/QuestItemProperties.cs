using System.Collections.Generic;

namespace GameItemEditor.Core.Models
{
    public class QuestItemProperties
    {
        public string QuestId { get; set; }
        public bool IsKey { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Custom { get; set; } = new();
    }
}
