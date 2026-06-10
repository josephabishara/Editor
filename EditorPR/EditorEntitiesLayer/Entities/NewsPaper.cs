using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class NewsPaper : BaseEntity
    {
     
        public int PublicationId { get; set; }

        public DateTime Date { get; set; }
        public string Title { get; set; }
        public decimal PRValue { get; set; }
        public decimal ADValue { get; set; }
     
        public string ArticleBranding { get; set; } // Branded, Unbranded, N/A
        public string HeadlineBranding { get; set; } // Branded, Unbranded, N/A
        
        public bool pictureInArticle { get; set; } = false;

        public bool Generation { get; set; } =false;
        public string? Toning { get; set; } // None, Positive,  Neutral, Negative
        public string? Content { get; set; } // text content of the news article, optional
        public string? Images { get; set; }  // ← added: JSON array of paths


    }
}
