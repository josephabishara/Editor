using EditorViewModelLayer.MediaViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.GeneralNewspaperViewModel
{
    public class GeneralNewspaperDTO
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Publication")]
        public int PublicationId { get; set; }
        public string? PublicationName { get; set; }   // display only

        [Required]
        [Display(Name = "Writer")]
        public int WriterId { get; set; }
        public string? WriterName { get; set; }         // display only

        [Display(Name = "Pages")]
        [Range(0, 9999)]
        public int Pages { get; set; }

        [Display(Name = "Height (cm)")]
        [Range(0, double.MaxValue)]
        public decimal Height { get; set; }

        [Display(Name = "Width (cm)")]
        [Range(0, double.MaxValue)]
        public decimal Width { get; set; }

        [Display(Name = "Article Branding")]
        [MaxLength(20)]
        public string? ArticleBranding { get; set; }    // Branded | Not Branded | N/A

        [Display(Name = "Headline Branding")]
        [MaxLength(20)]
        public string? HeadlineBranding { get; set; }   // Branded | Not Branded | N/A

        [Display(Name = "Picture in Article")]
        [MaxLength(10)]
        public string? PictureinArticle { get; set; }   // Yes | No

        [Display(Name = "AI Generated")]
        public bool Generation { get; set; }

        [Display(Name = "Toning")]
        public string? Toning { get; set; }             // None | Positive | Neutral | Negative

        [Required]
        [Display(Name = "Title")]
        [MaxLength(500)]
        public string? Title { get; set; }

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Display(Name = "Images")]
        public string? Images { get; set; }

        // ── Dropdown data (populated by controller, never posted back) ────────
        public List<MediaSelectOption> PublicationOptions { get; set; } = new();
        public List<MediaSelectOption> WriterOptions { get; set; } = new();
        public List<MediaSelectOption> BrandingOptions { get; set; } = new();
        public List<MediaSelectOption> ToningOptions { get; set; } = new();
        public List<MediaSelectOption> YesNoOptions { get; set; } = new();
    }
}
