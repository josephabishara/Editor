using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ChannelViewModel
{
    public class ChannelDTO
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Channel Name")]
        public string ChannelName { get; set; } = string.Empty;

        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }       // A, B, C

        [Display(Name = "Reach")]
        public string? ChannelReach { get; set; }

        [Display(Name = "Distribution")]
        public string? Distribution { get; set; }

        [Display(Name = "Language")]
        public string? ChannelLanguage { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be positive.")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Currency must be positive.")]
        [Display(Name = "Unit Currency")]
        public decimal UnitCurrency { get; set; }
    }
}
