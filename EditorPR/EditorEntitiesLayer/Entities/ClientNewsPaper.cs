using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ClientNewsPaper : BaseEntity
    {
      
        public int NewsPaperId { get; set; }// reference  not FK

        public int ClientId { get; set; } // FK → Client and Required

        public int PublicationId { get; set; } // FK → Publication and Required


        public DateTime Date { get; set; }
        // ── Source fields ───────────────────────────────────────────────────────
      
        public int CategoryId { get; set; }      // FK → ClientCategories
        public int SubCategoryId { get; set; }   // FK → ClientCategories (child)
        public int WriterId { get; set; }         // FK → Writer

        // ── Layout / measurement ────────────────────────────────────────────────
        public int Pages { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }

        // ── Content fields ──────────────────────────────────────────────────────
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }
        public string? Content { get; set; } // text content of the news article, optional

        public decimal PRValue { get; set; } // = ADValue * 3.5
        public decimal ADValue { get; set; } // is the CM Price in Publication 

        [MaxLength(20)]
        public string ArticleBranding { get; set; } // Branded, Unbranded, N/A
        [MaxLength(20)]
        public string HeadlineBranding { get; set; } // Branded, Unbranded, N/A

        public bool pictureInArticle { get; set; } = false;

        public bool Generation { get; set; } = false;
        public string? Toning { get; set; } // None, Positive,  Neutral, Negative




        // ── Publish flag (Admin only) ───────────────────────────────────────────

        public bool Publish { get; set; } = false;


        // ── Navigation ─────────────────────────────────────────────────────────
        [ForeignKey(nameof(NewsPaperId))]
        public NewsPaper NewsPaper { get; set; } = null!;

        [ForeignKey(nameof(ClientId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(WriterId))]
        public Writer? Writer { get; set; }


    }
}
