using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ClientViewModel
{
    public class PublicationCustomerCategoryDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int PublicationId { get; set; }

        // ── Read-only display ────────────────────────────────────────────────────
        public string PublicationName { get; set; } = string.Empty;
        public string? PublicationURL { get; set; } = string.Empty;

        // ── Editable ─────────────────────────────────────────────────────────────
        [Display(Name = "Media Type")]
        public string? MediaType { get; set; }

        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }

        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

       

        [Display(Name = "Distribution")]
        public string? Distribution { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Unit Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be positive.")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Circulation")]
        public int? Circulation { get; set; }
    }

    public class UpdateClientPublicationCategoriesDTO
    {
        public int CustomerId { get; set; }
        public List<PublicationCustomerCategoryDTO> Categories { get; set; } = new();
    }
}
