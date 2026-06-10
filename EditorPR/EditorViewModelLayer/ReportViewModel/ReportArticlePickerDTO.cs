using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.ReportViewModel
{
    public class ReportArticlePickerDTO
    {
        public int ArticleId { get; set; }

        public DateTime Date { get; set; }

        public string? Title { get; set; }

        public string? ArticleURL { get; set; }

        public string? WebsiteName { get; set; }    // GeneralArticle.Website.WebsiteName

        public string? WriterName { get; set; }     // GeneralArticle.Writer.WriterName

        public string? Language { get; set; }

        public string? ArticleBranding { get; set; }


        public decimal PRValue { get; set; }

        public decimal ADValue { get; set; }

        /// <summary>
        /// True when this article is already linked to the current report.
        /// Drives the pre-checked state of the checkbox on GET.
        /// </summary>
        public bool Selected { get; set; }
    }
}
