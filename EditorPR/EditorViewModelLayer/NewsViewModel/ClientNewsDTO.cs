using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorViewModelLayer.NewsViewModel
{
    // ── Static option lists ────────────────────────────────────────────────────
    public static class NewsOptions
    {
        public static readonly string[] SourceTypes = { "Publication", "Article", "Video" };
        public static readonly string[] BrandingOpts = { "Branded", "Unbranded", "N/A" };
        public static readonly string[] ToningOpts = { "None", "Positive", "Neutral", "Negative" };
        public static readonly string[] TranslationOpts = { "None", "Summary", "Full" };

        // Pre-built SelectListItem collections for static options
        public static List<SelectListItem> BrandingSelectList() =>
            BrandingOpts.Select(o => new SelectListItem(o, o)).ToList();

        public static List<SelectListItem> ToningSelectList() =>
            ToningOpts.Select(o => new SelectListItem(o, o)).ToList();

        public static List<SelectListItem> TranslationSelectList() =>
            TranslationOpts.Select(o => new SelectListItem(o, o)).ToList();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // ClientNewsDTO
    // ──────────────────────────────────────────────────────────────────────────
    public class ClientNewsDTO
    {
        public int Id { get; set; }
        public int NewsId { get; set; }
        public int ClientId { get; set; }

        [Display(Name = "Client")]
        public string? ClientName { get; set; }

        [Display(Name = "News Mode")]
        public string NewsMode { get; set; } = "New"; // New | Existing

        [Display(Name = "Existing News")]
        public int? ExistingNewsId { get; set; }

        [Required]
        [Display(Name = "Source Type")]
        public string SourceType { get; set; } = "Publication";

        [Display(Name = "Source")]
        public int publicationId { get; set; }

        [Display(Name = "Source Name")]
        public string? SourceName { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        [Display(Name = "Writer")]
        public int WriterId { get; set; }

        public string? WriterName { get; set; }

        [Display(Name = "Pages")]
        [Range(0, 9999)]
        public int Pages { get; set; }

        [Display(Name = "Height (cm)")]
        [Range(0, double.MaxValue)]
        public decimal Height { get; set; }

        [Display(Name = "Width (cm)")]
        [Range(0, double.MaxValue)]
        public decimal Width { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [MaxLength(500)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "PR Value")]
        [Range(0, double.MaxValue)]
        public decimal PRValue { get; set; }

        [Display(Name = "AD Value")]
        [Range(0, double.MaxValue)]
        public decimal ADValue { get; set; }

        [Display(Name = "PR Option")]
        public string? PROption { get; set; }

        [Display(Name = "AD Option")]
        public string? ADOption { get; set; }

        [Display(Name = "Article Branding")]
        public string ArticleBranding { get; set; } = "N/A";

        [Display(Name = "Headline Branding")]
        public string HeadlineBranding { get; set; } = "N/A";

        [Display(Name = "Picture in Article")]
        public bool pictureInArticle { get; set; } = false;

        [Display(Name = "Generation")]
        public bool Generation { get; set; } = false;

        [Display(Name = "Toning")]
        public string? Toning { get; set; }

        [Display(Name = "Translation")]
        public string? Translation { get; set; }

        [Display(Name = "Published")]
        public bool Publish { get; set; } = false;

        // ── SelectListItem dropdowns (populated by service, never posted) ──────
        public List<SelectListItem> SourceSelectList { get; set; } = new();
        public List<SelectListItem> CategorySelectList { get; set; } = new();
        public List<SelectListItem> SubCategorySelectList { get; set; } = new();
        public List<SelectListItem> WriterSelectList { get; set; } = new();
        public List<SelectListItem> ExistingNewsSelectList { get; set; } = new();
        public List<SelectListItem> BrandingSelectList { get; set; } = new();
        public List<SelectListItem> ToningSelectList { get; set; } = new();
        public List<SelectListItem> TranslationSelectList { get; set; } = new();
    }

    // ── Dashboard list wrapper ─────────────────────────────────────────────────
    public class ClientNewsListDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPhoto { get; set; }

        public IEnumerable<ClientNewsDTO> Items { get; set; } = new List<ClientNewsDTO>();

        // Filter-panel SelectListItem lists (populated in controller Index action)
        public List<SelectListItem> CategorySelectList { get; set; } = new();
        public List<SelectListItem> SubCategorySelectList { get; set; } = new();
        public List<SelectListItem> WriterSelectList { get; set; } = new();
        public List<SelectListItem> PublicationSelectList { get; set; } = new();

        // Live counters
        public int TotalNews => Items.Count();
        public int TotalPublished => Items.Count(i => i.Publish);
        public int Publications => Items.Count(i => i.SourceType == "Publication");
        public int Articles => Items.Count(i => i.SourceType == "Article");
        public int Videos => Items.Count(i => i.SourceType == "Video");
        public decimal TotalPR => Items.Sum(i => i.PRValue);
        public decimal TotalAD => Items.Sum(i => i.ADValue);
    }

    // ── Lightweight DTOs used only inside service layer ────────────────────────
    public class SourceItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? MediaTier { get; set; }
    }

    public class CategoryItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class WriterItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ExistingNewsItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string SourceType { get; set; } = string.Empty;
    }
}
