using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ClientNewsPaper : BaseEntity
    {
        [Required] 
        public int NewsPaperId { get; set; }
        
        [Required]
        public int ClientId { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public decimal PRValue { get; set; }
        public decimal ADValue { get; set; }
        public string PROption { get; set; }
        public string ADOption { get; set; }
        public string ArticleBranding { get; set; } // Branded, Unbranded, N/A
        public string HeadlineBranding { get; set; } // Branded, Unbranded, N/A

        public bool pictureInArticle { get; set; } = false;

        public bool Generation { get; set; } = false;
        public string? Toning { get; set; } // None, Positive,  Neutral, Negative
        public string? Translation { get; set; } // Empty, None, Summry, Full


    }
}
