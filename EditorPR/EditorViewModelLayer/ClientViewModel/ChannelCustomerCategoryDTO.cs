using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ClientViewModel
{
    public class ChannelCustomerCategoryDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ChannelId { get; set; }

        // ── Read-only display ────────────────────────────────────────────────────
        public string ChannelName { get; set; } = string.Empty;

        // ── Editable ─────────────────────────────────────────────────────────────
        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }

        [Display(Name = "Reach")]
        public int Reach { get; set; }

        [Display(Name = "Distribution")]
        public string? Distribution { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Unit Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be positive.")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Unit Currency")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Currency must be positive.")]
        public decimal UnitCurrency { get; set; }
    }

    public class UpdateClientChannelCategoriesDTO
    {
        public int CustomerId { get; set; }
        public List<ChannelCustomerCategoryDTO> Categories { get; set; } = new();
    }

}
