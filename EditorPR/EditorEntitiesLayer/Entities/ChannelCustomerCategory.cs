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

        [MaxLength(10)]
        public string? MediaTier { get; set; } // editable — default copied from Channel on creation

        // ── Navigation ──────────────────────────────────────────────────────
        [ForeignKey(nameof(CustomerId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(ChannelId))]
        public Channel Channel { get; set; } = null!;
    }

}
