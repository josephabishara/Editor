using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ChannelCustomerCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }   // FK → Client

        [Required]
        public int ChannelId { get; set; }    // FK → Channel

        // ── Editable fields — defaults copied from Channel on creation ────────────
        [MaxLength(10)]
        public string? MediaTier { get; set; }

        [MaxLength(200)]
        public int Reach { get; set; }      // source: Channel.ChannelReach

        [MaxLength(200)]
        public string? Distribution { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; }   // source: Channel.ChannelLanguage

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCurrency { get; set; }


        // ── Navigation ──────────────────────────────────────────────────────
        [ForeignKey(nameof(CustomerId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(ChannelId))]
        public Channel Channel { get; set; } = null!;
    }

}
