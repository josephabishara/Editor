using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.GeneralArticleViewModel
{
    // ── One client's choice within the Share modal (Category/SubCategory are
    //    per-client because ClientCategories is scoped by ClientId) ───────────
    public class ShareClientRowDTO
    {
        public int ClientId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
    }

    // ── Posted from Details.cshtml when the user confirms the share modal ────
    public class ShareArticleToClientsDTO
    {
        public int GeneralArticleId { get; set; }
        public List<ShareClientRowDTO> Clients { get; set; } = new();
    }

    // ── Populates the client checklist inside the Share modal ────────────────
    public class ShareClientOptionDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        // True if this client already has a ClientArticle linked to this
        // GeneralArticle — UI disables the row / shows a "Shared" badge to
        // prevent duplicate ClientArticle rows for the same client+article.
        public bool AlreadyShared { get; set; }
    }
}
