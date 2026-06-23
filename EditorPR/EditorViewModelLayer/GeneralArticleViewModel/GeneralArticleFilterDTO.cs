using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.GeneralArticleViewModel
{
    public class GeneralArticleFilterDTO
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Title { get; set; }
        public int? WebsiteId { get; set; }
    }

}
