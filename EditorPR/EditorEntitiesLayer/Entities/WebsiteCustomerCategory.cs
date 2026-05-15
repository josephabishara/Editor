using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class WebsiteCustomerCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }  // FK → Client

        [Required]
        public int WebsiteId { get; set; }   // FK → Websites

        [MaxLength(10)]
        public string? MediaTier { get; set; } // editable — default copied from Website on creation

        // ── Navigation ──────────────────────────────────────────────────────
        [ForeignKey(nameof(CustomerId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(WebsiteId))]
        public Websites Website { get; set; } = null!;
    }
}
