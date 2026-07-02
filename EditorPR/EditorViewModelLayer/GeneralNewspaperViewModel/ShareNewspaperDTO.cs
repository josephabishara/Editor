using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.GeneralNewspaperViewModel
{
    // ── One client's choice within the Share modal (Category/SubCategory are
    //    per-client because ClientCategories is scoped by ClientId) ───────────
    public class ShareNewspaperClientRowDTO
    {
        public int ClientId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
    }

    // ── Posted from Details.cshtml when the user confirms the share modal ────
    public class ShareNewspaperToClientsDTO
    {
        public int GeneralNewspaperId { get; set; }
        public List<ShareNewspaperClientRowDTO> Clients { get; set; } = new();
    }

    // ── Populates the client checklist inside the Share modal ────────────────
    public class ShareNewspaperClientOptionDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        // True if this client already has a ClientNewsPaper linked to this
        // GeneralNewspaper — UI disables the row / shows a "Shared" badge to
        // prevent duplicate ClientNewsPaper rows for the same client+newspaper.
        public bool AlreadyShared { get; set; }
    }
}
