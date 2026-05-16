using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class News : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string SourceType { get; set; } = "Publication"; // Publication | Article | Video

        // ── Matches exactly the ClientNews fields ───────────────────────────────
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        public decimal PRValue { get; set; }
        public decimal ADValue { get; set; }

        [MaxLength(50)]
        public string? PROption { get; set; }

        [MaxLength(50)]
        public string? ADOption { get; set; }

        [MaxLength(20)]
        public string ArticleBranding { get; set; } = "N/A";

        [MaxLength(20)]
        public string HeadlineBranding { get; set; } = "N/A";

        public bool pictureInArticle { get; set; } = false;
        public bool Generation { get; set; } = false;

        [MaxLength(20)]
        public string? Toning { get; set; }

        [MaxLength(20)]
        public string? Translation { get; set; }

        // ── Navigation ─────────────────────────────────────────────────────────
        public ICollection<ClientNews> ClientNewsList { get; set; } = new List<ClientNews>();

    }
}
