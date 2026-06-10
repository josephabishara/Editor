using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class ChildNewsPaperDTO
    {
        public int Id { get; set; }  // 0 = new row, > 0 = existing (Edit mode)

        // ── Fields unique per child ────────────────────────────────────────────
        [Required]
        [Display(Name = "Publication")]
        public int PublicationId { get; set; }
        public string? PublicationName { get; set; }  // display only

        [Display(Name = "Writer")]
        public int WriterId { get; set; }
        public string? WriterName { get; set; }       // display only

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Pages")]
        public int Pages { get; set; }

        [Display(Name = "Height (cm)")]
        public decimal Height { get; set; }

        [Display(Name = "Width (cm)")]
        public decimal Width { get; set; }

        // ── Auto-filled from child's Publication ───────────────────────────────
        public decimal ADValue { get; set; }
        public decimal PRValue { get; set; }
        public string? MediaType { get; set; }
        public string? MediaTier { get; set; }
        public string? Frequency { get; set; }
        public string? Language { get; set; }
        public int? Circulation { get; set; }
        public int Reach { get; set; }

        // ── Dropdown options (populated by controller) ─────────────────────────
        public List<MediaSelectOption> PublicationOptions { get; set; } = new();
        public List<MediaSelectOption> WriterOptions { get; set; } = new();
    }
}
