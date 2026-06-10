using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ReportArticle : BaseEntity
    {
        public int ReportId { get; set; }
        public int ArticleId { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────────
        [ForeignKey(nameof(ReportId))]
        public Report? Report { get; set; }

        [ForeignKey(nameof(ArticleId))]
        public GeneralArticle? Article { get; set; }
    }
}
