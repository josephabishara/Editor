using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class Channel : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string ChannelName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? MediaTier { get; set; }       // A, B, C

        [MaxLength(200)]
        public string? ChannelReach { get; set; }

        [MaxLength(200)]
        public string? Distribution { get; set; }

        [MaxLength(50)]
        public string? ChannelLanguage { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal UnitCurrency { get; set; }
    }
}
