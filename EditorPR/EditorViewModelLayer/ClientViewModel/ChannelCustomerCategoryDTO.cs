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

        // Display only — from Channel navigation
        public string ChannelName { get; set; } = string.Empty;

        // Editable
        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }
    }

    public class UpdateClientChannelCategoriesDTO
    {
        public int CustomerId { get; set; }
        public List<ChannelCustomerCategoryDTO> Categories { get; set; } = new();
    }

}
