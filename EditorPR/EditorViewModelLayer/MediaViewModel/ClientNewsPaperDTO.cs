using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class ClientNewsPaperDTO
    {
        public int Id { get; set; }
        public int NewsPaperId { get; set; }

        [Required]
        public int ClientId { get; set; }
        public string? ClientName { get; set; }

        // ── Parent / Child ─────────────────────────────────────────────────────
        public int? ParentId { get; set; }
        public List<ChildNewsPaperDTO> Children { get; set; } = new();


        // ── Source ────────────────────────────────────────────────────────────
        [Required]
        [Display(Name = "Publication")]
        public int PublicationId { get; set; }
        public string? PublicationName { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }        // ← display only

        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }     // ← display only

        [Display(Name = "Writer")]
        public int WriterId { get; set; }
        public string? WriterName { get; set; }

        // ── Auto-filled from Publication ──────────────────────────────────────
        [Display(Name = "Media Type")]
        public string? MediaType { get; set; }

        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }

        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Circulation")]
        public int? Circulation { get; set; }

        [Display(Name = "Reach")]
        public int Reach { get; set; }

        // ── Layout ────────────────────────────────────────────────────────────
        [Display(Name = "Pages")]
        public int Pages { get; set; }

        [Display(Name = "Height (cm)")]
        public decimal Height { get; set; }

        [Display(Name = "Width (cm)")]
        public decimal Width { get; set; }

        // ── Values ────────────────────────────────────────────────────────────
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;


        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "AD Value")]
        public decimal ADValue { get; set; }

        [Display(Name = "PR Value")]
        public decimal PRValue { get; set; }

        // ── Branding & Analysis ───────────────────────────────────────────────
        [Display(Name = "Article Branding")]
        public string ArticleBranding { get; set; } = "Branded";

        [Display(Name = "Headline Branding")]
        public string HeadlineBranding { get; set; } = "Branded";
        [Display(Name = "Picture in Article")]
        public string? PictureinArticle { get; set; } = "Yes";

        [Display(Name = "Generation")]
        public string? Generation { get; set; } = "Generated";

        [Display(Name = "Toning")]
        public string? Toning { get; set; }

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Display(Name = "Images")]
        public string? Images { get; set; }

        [Display(Name = "Published")]
        public bool Publish { get; set; } = false;

        // ── Audit ─────────────────────────────────────────────────────────────
        public string? CreatedByUserName { get; set; }   // ← display only
        public DateTime CreatedAt { get; set; }           // ← from BaseEntity

        // ── Dropdowns ─────────────────────────────────────────────────────────
        public List<MediaSelectOption> PublicationOptions { get; set; } = new();
        public List<MediaSelectOption> CategoryOptions { get; set; } = new();
        public List<MediaSelectOption> SubCategoryOptions { get; set; } = new();
        public List<MediaSelectOption> WriterOptions { get; set; } = new();
        public List<MediaSelectOption> BrandingOptions { get; set; } = new();
        public List<MediaSelectOption> ToningOptions { get; set; } = new();
        public List<MediaSelectOption> YesNoOptions { get; set; } = new();
        public List<MediaSelectOption> GenerationOptions { get; set; } = new();
    }
}
