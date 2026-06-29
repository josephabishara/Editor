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

        public int PublicationId { get; set; }

        public string? PublicationName { get; set; }

        public string? WriterName { get; set; }

        // ── Category / SubCategory — drives report grouping & sort order ────────
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public int SubCategoryId { get; set; }

        public string? SubCategoryName { get; set; }

        public int CategoryOrder { get; set; }

        public int SubCategoryOrder { get; set; }

        // ── Content — used by the Preview page ───────────────────────────────────
        public string? Content { get; set; }

        public List<string> ImagePaths { get; set; } = new();   // reserved — populated if/when scanned clippings are added

        public decimal PRValue { get; set; }

        public decimal ADValue { get; set; }

        public string? ArticleBranding { get; set; }

        public string? HeadlineBranding { get; set; }

        public string? Toning { get; set; }

        public string? MediaType { get; set; }

        public string? MediaTier { get; set; }

        public string? Frequency { get; set; }

        public int? Circulation { get; set; }
        public int PageNumber { get; set; }

        public int Reach { get; set; }

        // ,[Height]      ,[Width]
        public decimal Height { get; set; }
        public decimal Width { get; set; }

        // Language  
        public string? Language { get; set; }

        public bool Generation { get; set; }  

        public string? PictureinArticle { get; set; }

        /// <summary>
        /// True when this newspaper is already linked to the current report.
        /// Drives the pre-checked state of the checkbox on GET.
        /// </summary>
        public bool Selected { get; set; }
    }
}
