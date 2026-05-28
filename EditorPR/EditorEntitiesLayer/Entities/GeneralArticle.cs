using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class GeneralArticle : BaseEntity
    {
       public DateTime Date { get; set; } // from view
        public int WebsiteId { get; set; } // only Website’s Foreign Key  
        public int WriterId { get; set; } // FK → Writer
        public string? Language { get; set; }
        public string? ArticleBranding { get; set; } //  Branded ,  Not Branded,  N/A
        public string? HeadlineBranding { get; set; } //  Branded,  Not Branded,  N/A
        public string? PictureinArticle { get; set; } //  Yes ,  No
        public bool? Generation { get; set; } //  Generated,  Not Generated
        public string? ArticleURL { get; set; }
        public string? Title { get; set; } // is the Header of article 
        public string? Content { get; set; } // is a text area as  TinyMCE free edition 
        public string? Images { get; set; } // can select multi images


        // ── Navigation ─────────────────────────────────────────────────────
        [ForeignKey(nameof(WebsiteId))]
        public Websites? Website { get; set; }

        [ForeignKey(nameof(WriterId))]
        public Writer? Writer { get; set; }


    }
}
