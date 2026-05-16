using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ClientViewModel
{
    public class ClientCategoryDTO
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }
        public string? ClientName { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(200, ErrorMessage = "Category name cannot exceed 200 characters.")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Parent Category")]
        public int? ParentCategory { get; set; }
        public string? ParentCategoryName { get; set; }

        [Display(Name = "Category Type")]
        public string? CategoryType { get; set; }

        public string? Status { get; set; }

        [Display(Name = "Display Order")]
        public int Order { get; set; }

        [Display(Name = "Article Count")]
        public int ArticleCount { get; set; }

        // For dropdowns
        public IEnumerable<ClientCategoryDTO> SubCategories { get; set; }
            = new List<ClientCategoryDTO>();
    }
}
