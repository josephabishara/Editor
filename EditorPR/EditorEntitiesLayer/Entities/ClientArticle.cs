using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace EditorEntitiesLayer.Entities
{
    public class ClientArticle : BaseEntity
    {
        public int ArticleId { get; set; } // reference  not FK
        public int ClientId { get; set; } // FK → Client and Required
        public DateTime Date { get; set; } // from view
        public int WebsiteId { get; set; } // only Website’s Foreign Key  
        public string? WebsiteType { get; set; }
        public int CategoryId { get; set; } // FK → ClientCategories
        public int SubCategoryId { get; set; } // FK → ClientCategories (child)
        public int WriterId { get; set; } // FK → Writer

        // ── Publish flag (Admin only) ───────────────────────────────────────────

        public bool Publish { get; set; } = false;
        public int? ParentId { get; set; }
        public string? Frequency { get; set; } //	is the Frequency of Website
        public string? MediaType { get; set; } // newspaper, magazine, journal, newsletter (get from Website => by id )
        public int Impression { get; set; } // is the Impression  of Website
        public int Reach { get; set; } // is Impression  * 4 
        public decimal? ADValue { get; set; } // is the UnitPrice in Website
        public decimal? PRValue { get; set; } // = ADValue * 3.5
        public string? Toning { get; set; } // None, Positive,  Neutral, Negative
        public string? MediaTier  { get; set; } //  is the Media Tier  of WebsiteCustomerCategory
        public string? Language { get; set; }
        public string? Writer { get; set; }
        public string? ArticleBranding { get; set; } //  Branded ,  Not Branded,  N/A
        public string? HeadlineBranding { get; set; } //  Branded,  Not Branded,  N/A
        public string? PictureinArticle { get; set; } //  Yes ,  No
        public bool? Generation { get; set; } //  Generated,  Not Generated
        public string? ArticleURL { get; set; }
        public string? Title { get; set; } // is the Header of article 
        public string? Content { get; set; } // is a text area as  TinyMCE free edition 
        public string? Images { get; set; } // can select multi images

        // ── Navigation ─────────────────────────────────────────────────────────
        [ForeignKey(nameof(ParentId))]
        public ClientArticle? Parent { get; set; }

        public ICollection<ClientArticle> Children { get; set; } = new List<ClientArticle>();


    }
}
