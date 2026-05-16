using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class Publication : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Publication Name")]
        public string PublicationName { get; set; } = string.Empty;

   
        [MaxLength(50)]
        [Display(Name = "Media Type")]
        public string? MediaType { get; set; }   // newspaper, magazine, journal, newsletter

        [MaxLength(10)]
        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }   // A, B, C

        [MaxLength(50)]
        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }   // daily, weekly, monthly

     

        [MaxLength(200)]
        public string? Distribution { get; set; }

        [MaxLength(50)]

        public string? Language { get; set; }

        [Display(Name = "CM Price")]
        public decimal CmPrice { get; set; }

        public int? Circulation { get; set; }
    }
}
