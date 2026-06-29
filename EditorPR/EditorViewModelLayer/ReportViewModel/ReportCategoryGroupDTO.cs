using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.ReportViewModel
{
    public class ReportCategoryGroupDTO
    {
        public string CategoryName { get; set; } = string.Empty;

        public string? SubCategoryName { get; set; }   // null when the item has no subcategory

        public List<ReportArticlePickerDTO> Articles { get; set; } = new();

        public List<ReportNewspaperPickerDTO> Newspapers { get; set; } = new();

        public int ItemCount => Articles.Count + Newspapers.Count;

        public decimal GroupPRValue => Articles.Sum(a => a.PRValue) + Newspapers.Sum(n => n.PRValue);

        public decimal GroupADValue => Articles.Sum(a => a.ADValue) + Newspapers.Sum(n => n.ADValue);
    }
}
