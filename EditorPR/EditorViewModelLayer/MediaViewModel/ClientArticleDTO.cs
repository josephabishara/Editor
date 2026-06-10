using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class ClientArticleDTO
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }  // reference, not FK

        [Required]
        public int ClientId { get; set; }
        public string? ClientName { get; set; }

        // ── Source ────────────────────────────────────────────────────────────
        [Required]
        [Display(Name = "Website")]
        public int WebsiteId { get; set; }
        public string? WebsiteName { get; set; }
        public string? WebsiteType { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }       // ← display only

        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }    // ← display only

        [Display(Name = "Writer")]
        public int WriterId { get; set; }
        public string? WriterName { get; set; }

        // ── Auto-filled from Website ──────────────────────────────────────────
        [Display(Name = "Media Type")]
        public string? MediaType { get; set; }

        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }  // from WebsiteCustomerCategory

        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Impression")]
        public int Impression { get; set; }

        [Display(Name = "Reach")]
        public int Reach { get; set; }   // Impression * 4

        // ── Values ────────────────────────────────────────────────────────────
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [MaxLength(500)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "AD Value")]
        public decimal ADValue { get; set; }   // UnitPrice from Website

        [Display(Name = "PR Value")]
        public decimal PRValue { get; set; }   // ADValue * 3.5

        // ── Branding & Analysis ───────────────────────────────────────────────
        [Display(Name = "Article Branding")]
        public string? ArticleBranding { get; set; } = "Brnded";

        [Display(Name = "Headline Branding")]
        public string? HeadlineBranding { get; set; } = "Brnded";

        [Display(Name = "Picture in Article")]
        public string? PictureinArticle { get; set; } = "Yes";

        [Display(Name = "Generation")]
        public string? Generation { get; set; } = "Generated";

        [Display(Name = "Toning")]
        public string? Toning { get; set; }

        [Display(Name = "Article URL")]
        public string? ArticleURL { get; set; }

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Display(Name = "Images")]
        public string? Images { get; set; }

        public int? ParentId { get; set; }
        // ── Audit ─────────────────────────────────────────────────────────────
        public string? CreatedByUserName { get; set; }  // ← display only
        public DateTime CreatedAt { get; set; }          // ← from BaseEntity

        // ── Publish flag (Admin only) ───────────────────────────────────────────
        public bool Publish { get; set; } = false;

        // Children entered on the Create / Edit form
        // Each child shares all parent data except Website, Writer, Date, ArticleURL
        public List<ChildArticleDTO> Children { get; set; } = new();

        // ── Dropdown lists ────────────────────────────────────────────────────
        public List<MediaSelectOption> WebsiteOptions { get; set; } = new();
        public List<MediaSelectOption> CategoryOptions { get; set; } = new();
        public List<MediaSelectOption> SubCategoryOptions { get; set; } = new();
        public List<MediaSelectOption> WriterOptions { get; set; } = new();
        public List<MediaSelectOption> BrandingOptions { get; set; } = new();
        public List<MediaSelectOption> ToningOptions { get; set; } = new();
        public List<MediaSelectOption> YesNoOptions { get; set; } = new();
        public List<MediaSelectOption> GenerationOptions { get; set; } = new();
    }
}
