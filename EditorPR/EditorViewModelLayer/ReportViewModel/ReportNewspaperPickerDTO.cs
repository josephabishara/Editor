using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.ReportViewModel
{
    public class ReportNewspaperPickerDTO
    {
        public int NewspaperId { get; set; }

        public DateTime Date { get; set; }

        public string? Title { get; set; }

        // Publication info — NewsPaper has PublicationId FK; Publication has PublicationName
        public int PublicationId { get; set; }

        public string? PublicationName { get; set; }    // NewsPaper.Publication.PublicationName

        public decimal PRValue { get; set; }

        public decimal ADValue { get; set; }

        public string? ArticleBranding { get; set; }

        public string? HeadlineBranding { get; set; }

        public string? Toning { get; set; }

        /// <summary>
        /// True when this newspaper is already linked to the current report.
        /// Drives the pre-checked state of the checkbox on GET.
        /// </summary>
        public bool Selected { get; set; }
    }
}
