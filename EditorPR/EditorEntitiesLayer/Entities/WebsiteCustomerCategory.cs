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
        public string? MediaTier { get; set; }

        [MaxLength(50)]
        public string? Frequency { get; set; }

        [MaxLength(200)]
        public string? Reach { get; set; }

        [MaxLength(200)]
        public string? Distribution { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        [ForeignKey(nameof(CustomerId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(WebsiteId))]
        public Websites Website { get; set; } = null!;
    }
}
