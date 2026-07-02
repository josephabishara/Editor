using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ReportNewspaper : BaseEntity
    {
        public int ReportId { get; set; }
        public int NewspaperId { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────────
        [ForeignKey(nameof(ReportId))]
        public Report? Report { get; set; }

        [ForeignKey(nameof(NewspaperId))]
        public ClientNewsPaper? NewsPaper { get; set; }
    }
}
