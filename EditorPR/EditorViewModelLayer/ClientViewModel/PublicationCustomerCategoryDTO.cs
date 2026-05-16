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

        // Display only — from Publication navigation
        public string PublicationName { get; set; } = string.Empty;
        public string PublicationURL { get; set; } = string.Empty;

        // Editable
        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }
    }

    public class UpdateClientPublicationCategoriesDTO
    {
        public int CustomerId { get; set; }
        public List<PublicationCustomerCategoryDTO> Categories { get; set; } = new();
    }
}
