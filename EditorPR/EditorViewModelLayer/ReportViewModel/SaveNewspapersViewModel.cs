using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ReportViewModel
{
    public class SaveNewspapersViewModel
    {
        public int ReportId { get; set; }
        public List<int> SelectedNewspaperIds { get; set; } = new();

        [DataType(DataType.Date)]
        public DateTime? From { get; set; }

        [DataType(DataType.Date)]
        public DateTime? To { get; set; }
    }
}
