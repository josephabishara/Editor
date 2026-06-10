using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ClientViewModel
{
    // ── Used for display + edit per row ───────────────────────────────────────
    public class WebsiteCustomerCategoryDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int WebsiteId { get; set; }

        // ── Read-only display (from Website navigation) ──────────────────────────
        public string WebsiteName { get; set; } = string.Empty;
        public string WebsiteURL { get; set; } = string.Empty;

        // ── Editable ─────────────────────────────────────────────────────────────
        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }

        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

        [Display(Name = "Reach")]
        public string? Reach { get; set; }

        [Display(Name = "Distribution")]
        public string? Distribution { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Unit Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be positive.")]
        public decimal UnitPrice { get; set; }
    }

    // ── Used when submitting the full categories list for a client ─────────────
    public class UpdateClientCategoriesDTO
    {
        public int CustomerId { get; set; }
        public List<WebsiteCustomerCategoryDTO> Categories { get; set; } = new();
    }
}
