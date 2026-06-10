using EditorEntitiesLayer.Entities;
using System.ComponentModel.DataAnnotations;

namespace EditorViewModelLayer.ReportViewModel
{
    public class ReportDTO
    {
        public int Id { get; set; }

        // ── Step 1 fields ───────────────────────────────────────────────────────

        [Required(ErrorMessage = "Customer is required.")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        public string? CustomerName { get; set; }   // display only — not posted

        [Required(ErrorMessage = "Report Name is required.")]
        [MaxLength(300)]
        [Display(Name = "Report Name")]
        public string ReportName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Report Date")]
        public DateTime ReportDate { get; set; } = DateTime.Today;

        [Display(Name = "Published")]
        public bool Publish { get; set; } = false;   // always false on create; toggled via Publish/UnPublish actions

        [Required]
        [Display(Name = "Report Type")]
        public ReportType ReportType { get; set; } = ReportType.Daily;

        // ── Summary counts — read-only, populated in List / Details ─────────────

        public int ArticleCount { get; set; }
        public int NewspaperCount { get; set; }
    }
}
