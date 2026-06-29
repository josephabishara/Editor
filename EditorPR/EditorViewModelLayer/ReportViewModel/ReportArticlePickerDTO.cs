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

        public string? WebsiteName { get; set; }

        public string? WriterName { get; set; }

        public string? Language { get; set; }

        public string? ArticleBranding { get; set; }

        public string? HeadlineBranding { get; set; }

        public string? MediaTier { get; set; }

        public string? MediaType { get; set; }

        // ── Category / SubCategory — drives report grouping & sort order ────────
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public int SubCategoryId { get; set; }

        public string? SubCategoryName { get; set; }

        public int CategoryOrder { get; set; }      // ClientCategories.Order — used for sort, not display

        public int SubCategoryOrder { get; set; }

        // ── Content & Images — used by the Preview page ─────────────────────────
        public string? Content { get; set; }         // TinyMCE HTML content

        public List<string> ImagePaths { get; set; } = new();   // deserialized from ClientArticle.Images (JSON array)

        // ── PR / AD values — from ClientArticle ──────────────────────────────────
        public decimal PRValue { get; set; }

        public decimal ADValue { get; set; }


        // Frequency, Impression, Reach, Toning, Pictureinarticle
        public string? Frequency { get; set; } //	is the Frequency of Website
        public int Impression { get; set; } // is the Impression  of Website
        public int Reach { get; set; } // is Impression  * 4 
        public string? Toning { get; set; } // None, Positive,  Neutral, Negative
        public string? PictureinArticle { get; set; } //  Yes ,  No
        public string? Generation { get; set; } //  Generated,  Not Generated


        /// <summary>
        /// True when this article is already linked to the current report.
        /// Drives the pre-checked state of the checkbox on GET.
        /// </summary>
        public bool Selected { get; set; }
    }
}
