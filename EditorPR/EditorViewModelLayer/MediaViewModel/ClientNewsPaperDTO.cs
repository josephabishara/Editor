using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class ClientNewsPaperDTO
    {
        public int Id { get; set; }
        public int NewsPaperId { get; set; }  // reference, not FK

        [Required]
        public int ClientId { get; set; }
        public string? ClientName { get; set; }

        // ── Source ────────────────────────────────────────────────────────────
        [Required]
        [Display(Name = "Publication")]
        public int PublicationId { get; set; }
        public string? PublicationName { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        [Display(Name = "Writer")]
        public int WriterId { get; set; }
        public string? WriterName { get; set; }

        // ── Auto-filled from Publication ──────────────────────────────────────
        [Display(Name = "Media Type")]
        public string? MediaType { get; set; }

        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }  // from PublicationCustomerCategory

        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Circulation")]
        public int? Circulation { get; set; }

        [Display(Name = "Reach")]
        public int Reach { get; set; }   // Circulation * 4

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
        public decimal ADValue { get; set; }   // CM Price from Publication

        [Display(Name = "PR Value")]
        public decimal PRValue { get; set; }   // ADValue * 3.5

        // ── Branding & Analysis ───────────────────────────────────────────────
        [Display(Name = "Article Branding")]
        public string ArticleBranding { get; set; } = "N/A";

        [Display(Name = "Headline Branding")]
        public string HeadlineBranding { get; set; } = "N/A";

        [Display(Name = "Toning")]
        public string? Toning { get; set; }

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Display(Name = "Images")]
        public string? Images { get; set; }  // comma-separated saved paths

        [Display(Name = "Published")]
        public bool Publish { get; set; } = false;

        // ── Dropdown lists (populated by service) ─────────────────────────────
        public List<MediaSelectOption> PublicationOptions { get; set; } = new();
        public List<MediaSelectOption> CategoryOptions { get; set; } = new();
        public List<MediaSelectOption> SubCategoryOptions { get; set; } = new();
        public List<MediaSelectOption> WriterOptions { get; set; } = new();
        public List<MediaSelectOption> BrandingOptions { get; set; } = new();
        public List<MediaSelectOption> ToningOptions { get; set; } = new();
    }
}
