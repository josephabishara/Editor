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

        // Display only — comes from Website navigation
        public string WebsiteName { get; set; } = string.Empty;
        public string WebsiteURL { get; set; } = string.Empty;

        // Editable
        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }
    }

    // ── Used when submitting the full categories list for a client ─────────────
    public class UpdateClientCategoriesDTO
    {
        public int CustomerId { get; set; }
        public List<WebsiteCustomerCategoryDTO> Categories { get; set; } = new();
    }
}
