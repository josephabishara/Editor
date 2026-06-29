using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.ReportViewModel
{
    public class ReportDetailsDTO : ReportDTO
    {
        public List<ReportArticlePickerDTO> Articles { get; set; } = new();

        public List<ReportNewspaperPickerDTO> Newspapers { get; set; } = new();

        /// <summary>
        /// Articles + Newspapers grouped by Category → SubCategory, sorted by
        /// ClientCategories.Order then by name. Each group renders as its own
        /// page in the Preview, with a heading showing the Category name.
        /// </summary>
        public List<ReportCategoryGroupDTO> CategoryGroups { get; set; } = new();

        // ── Client identity for cover page & footer ──────────────────────────────

        public string? CustomerLogoUrl { get; set; }       // Client.Photo — rendered in the report footer instead of the name

        public string? CustomerReportCoverPdfUrl { get; set; }   // Client.ReportCoverPdf — page 1 of the generated report

        // ── Computed totals — derived from the linked newspaper rows ────────────

        public decimal TotalPRValue => Newspapers.Sum(n => n.PRValue);

        public decimal TotalADValue => Newspapers.Sum(n => n.ADValue);
    }
}
