using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ClientViewModel
{
    public class ClientDashboardDTO
    {
        // Client identity
        public int ClientId { get; set; }
        public string? Photo { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; } 
        public string? Notes { get; set; }

        // ── Date filter (carried back to view for form state) ──────────────────
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

         
        // ── Articles (ClientArticle → Website) ─────────────────────────────
        public int ArticleCount { get; set; }
        public decimal ArticleTotalPR { get; set; }

        // ── Newspaper clippings (ClientNewsPaper → Publication) ────────────
        public int NewsPaperCount { get; set; }
        public decimal NewsPaperTotalPR { get; set; }

        // ── Videos (ClientVideo → Channel) ────────────────────────────────
        public int VideoCount { get; set; }
        public decimal VideoTotalAD { get; set; }
        public decimal VideoTotalPR { get; set; }

        // ── Grand totals ───────────────────────────────────────────────────
        public decimal GrandTotalPR => ArticleTotalPR + NewsPaperTotalPR + VideoTotalPR;



    }
}
