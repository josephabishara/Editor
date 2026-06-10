using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ReportViewModel
{
    public class SaveArticlesViewModel
    {
        public int ReportId { get; set; }
        public List<int> SelectedArticleIds { get; set; } = new();

        // Filter fields (GET only — not posted)
        [DataType(DataType.Date)]
        public DateTime? From { get; set; }

        [DataType(DataType.Date)]
        public DateTime? To { get; set; }
    }
}
