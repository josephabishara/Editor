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

        public int Publications { get; set; } = 0;
        public int Websites { get; set; }= 0;
        public int Videos { get; set; }= 0;
        public int TotalAD { get; set; }= 0;
        public int TotalPR { get; set; }= 0;
        public int TotalCirclation { get; set; } = 0;


       
        
      
    

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
