using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.ReportViewModel
{
    public class ReportDetailsDTO : ReportDTO
    {
        public List<ReportArticlePickerDTO> Articles { get; set; } = new();

        public List<ReportNewspaperPickerDTO> Newspapers { get; set; } = new();

        // ── Computed totals — derived from the linked newspaper rows ────────────

        public decimal TotalPRValue => Newspapers.Sum(n => n.PRValue);

        public decimal TotalADValue => Newspapers.Sum(n => n.ADValue);


    }
}
