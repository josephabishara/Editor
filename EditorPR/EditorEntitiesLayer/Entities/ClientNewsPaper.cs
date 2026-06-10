using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ClientNewsPaper : BaseEntity
    {
        public int NewsPaperId { get; set; }  // reference to NewsPaper master — not FK
        public int ClientId { get; set; }  // FK → Client
        public int PublicationId { get; set; }  // FK → Publication
        public DateTime Date { get; set; }

        // ── Source ─────────────────────────────────────────────────────────────
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int WriterId { get; set; }

        // ── Parent / Child ─────────────────────────────────────────────────────
        // NULL  = root (parent) clipping
        // > 0   = child clipping — same content, different Publication/Writer/Date/dimensions
        public int? ParentId { get; set; }

        // ── Layout / measurement ───────────────────────────────────────────────
        public int Pages { get; set; }
        public decimal Height { get; set; }  // cm
        public decimal Width { get; set; }  // cm

        // ── Content ────────────────────────────────────────────────────────────
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Images { get; set; }  // JSON array of saved paths

        // ── Values ─────────────────────────────────────────────────────────────
        public decimal? ADValue { get; set; }
        public decimal? PRValue { get; set; }

        // ── Branding & Analysis ────────────────────────────────────────────────
        [MaxLength(20)] public string ArticleBranding { get; set; } = "N/A";
        [MaxLength(20)] public string HeadlineBranding { get; set; } = "N/A";
        public bool pictureInArticle { get; set; } = false;
        public bool Generation { get; set; } = false;
        public string? Toning { get; set; }

        // ── Media info (auto-filled from Publication) ──────────────────────────
        public string? MediaType { get; set; }
        public string? MediaTier { get; set; }
        public string? Frequency { get; set; }
        public string? Language { get; set; }
        public int? Circulation { get; set; }
        public int Reach { get; set; }

        // ── Admin ──────────────────────────────────────────────────────────────
        public bool Publish { get; set; } = false;

        // ── Navigation ─────────────────────────────────────────────────────────
        [ForeignKey(nameof(NewsPaperId))]
        public NewsPaper NewsPaper { get; set; } = null!;

        [ForeignKey(nameof(ClientId))]
        public Client Client { get; set; } = null!;

        [ForeignKey(nameof(WriterId))]
        public Writer? Writer { get; set; }

        [ForeignKey(nameof(ParentId))]
        public ClientNewsPaper? Parent { get; set; }

        public ICollection<ClientNewsPaper> Children { get; set; } = new List<ClientNewsPaper>();
    }
}
