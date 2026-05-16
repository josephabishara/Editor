using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.PublicationViewModel
{
    public class PublicationDTO
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Publication Name")]
        public string PublicationName { get; set; } = string.Empty;

      

        [Display(Name = "Media Type")]
        public string? MediaType { get; set; }   // newspaper, magazine, journal, newsletter

        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }   // A, B, C

        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }   // daily, weekly, monthly



        [Display(Name = "Distribution")]
        public string? Distribution { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "CM Price must be positive.")]
        [Display(Name = "CM Price")]
        public decimal CmPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Circulation must be positive.")]
        [Display(Name = "Circulation")]
        public int? Circulation { get; set; }
    }
}
