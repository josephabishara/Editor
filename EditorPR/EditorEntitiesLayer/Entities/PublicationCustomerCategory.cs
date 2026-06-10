using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class PublicationCustomerCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }      // FK → Client

        [Required]
        public int PublicationId { get; set; }   // FK → Publication
      

        // ── Editable fields — defaults copied from Publication on creation ────────
        [MaxLength(50)]
        public string? MediaType { get; set; }  // newspaper, magazine, journal, newsletter

        [MaxLength(10)]
        public string? MediaTier { get; set; }  // A, B, C

        [MaxLength(50)]
        public string? Frequency { get; set; }

       

        [MaxLength(200)]
        public string? Distribution { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }  // default: Publication.CmPrice

        public int? Circulation { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        [ForeignKey(nameof(CustomerId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(PublicationId))]
        public Publication Publication { get; set; } = null!;
    }

}
