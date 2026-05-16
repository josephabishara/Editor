using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ClientNews : BaseEntity
    {
        [Required] 
        public int NewsId { get; set; }  // FK → News (master)

        [Required]
        public int ClientId { get; set; }  // FK → Client


        public DateTime Date { get; set; }

        // ── Source fields ───────────────────────────────────────────────────────
        public int publicationId { get; set; }   // FK: Publication | Websites | Channel (per SourceType)
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
        public decimal PRValue { get; set; }
        public decimal ADValue { get; set; }
        public string ADOption { get; set; }

        [MaxLength(50)]
        public string PROption { get; set; }
        [MaxLength(20)]
        public string ArticleBranding { get; set; } // Branded, Unbranded, N/A
        [MaxLength(20)]
        public string HeadlineBranding { get; set; } // Branded, Unbranded, N/A
        
        public bool pictureInArticle { get; set; } = false;

        public bool Generation { get; set; } = false;
        public string? Toning { get; set; } // None, Positive,  Neutral, Negative

        public string? Translation { get; set; } // Empty, None, Summry, Full







        // ── Publish flag (Admin only) ───────────────────────────────────────────

        public bool Publish { get; set; } = false;


        // ── Navigation ─────────────────────────────────────────────────────────
        [ForeignKey(nameof(NewsId))]
        public News News { get; set; } = null!;

        [ForeignKey(nameof(ClientId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(WriterId))]
        public Writer? Writer { get; set; }


    }
}
