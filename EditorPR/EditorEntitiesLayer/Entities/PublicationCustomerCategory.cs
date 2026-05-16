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

        [MaxLength(10)]
        public string? MediaTier { get; set; }   // editable — default copied from Publication on creation

        // ── Navigation ──────────────────────────────────────────────────────
        [ForeignKey(nameof(CustomerId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(PublicationId))]
        public Publication Publication { get; set; } = null!;
    }

}
