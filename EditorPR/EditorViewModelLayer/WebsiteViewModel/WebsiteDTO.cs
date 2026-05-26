using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.WebsiteViewModel
{
    public class WebsiteDTO
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Website Name")]
        [StringLength(200)]
        public string WebsiteName { get; set; } = string.Empty;

        [Required]
        [Url]
        [StringLength(500)]
        public string URL { get; set; } = string.Empty;

        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }

        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

        [Display(Name = "Impression")]
        public string? Impression { get; set; }

        [Display(Name = "Distribution")]
        public string? Distribution { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Required]
        [Display(Name = "Unit Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal UnitPrice { get; set; }
    }
}
