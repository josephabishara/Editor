using System;
using System.Collections.Generic;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class Websites: BaseEntity
    {
      
        public string WebsiteName { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public string? MediaTier { get; set; }
        public string? Frequency { get; set; }
        public string? Reach { get; set; }
        public string? Distribution { get; set; }
        public string? Language { get; set; }
        public decimal UnitPrice { get; set; }
   
    }
}
