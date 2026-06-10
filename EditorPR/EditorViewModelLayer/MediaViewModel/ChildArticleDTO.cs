using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class ChildArticleDTO
    {
        public int Id { get; set; }   // 0 = new, > 0 = existing (Edit mode)

        // ── Fields unique to each child ────────────────────────────────────────
        [Required]
        [Display(Name = "Website")]
        public int WebsiteId { get; set; }
        public string? WebsiteName { get; set; }   // display only

        [Required]
        [Display(Name = "Writer")]
        public int WriterId { get; set; }
        public string? WriterName { get; set; }    // display only

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Article URL")]
        public string? ArticleURL { get; set; }

        // ── Auto-filled from child's Website (same as parent logic) ───────────
        public string? Frequency { get; set; }
        public string? MediaType { get; set; }
        public int Impression { get; set; }
        public int Reach { get; set; }
        public decimal ADValue { get; set; }
        public decimal PRValue { get; set; }
        public string? MediaTier { get; set; }
        public string? Language { get; set; }

        // ── Dropdown options (populated by controller) ─────────────────────────
        public List<MediaSelectOption> WebsiteOptions { get; set; } = new();
        public List<MediaSelectOption> WriterOptions { get; set; } = new();
    }
}
