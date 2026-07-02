using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.GeneralNewspaperViewModel
{
    // ── Filter criteria for GeneralNewspaper Index ────────────────────────────
    public class GeneralNewspaperFilterDTO
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Title { get; set; }
        public int? PublicationId { get; set; }
    }
}
