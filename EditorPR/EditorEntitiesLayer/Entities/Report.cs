using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public enum ReportType
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    public class Report : BaseEntity
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(300)]
        public string ReportName { get; set; } = string.Empty;

        public DateTime ReportDate { get; set; } = DateTime.Today;

        public bool Publish { get; set; } = false;

        public ReportType ReportType { get; set; } = ReportType.Daily;

        // ── Navigation ──────────────────────────────────────────────────────────
        [ForeignKey(nameof(CustomerId))]
        public Client? Customer { get; set; }

        public ICollection<ReportArticle> ReportArticles { get; set; } = new List<ReportArticle>();
        public ICollection<ReportNewspaper> ReportNewspapers { get; set; } = new List<ReportNewspaper>();
    }
}
