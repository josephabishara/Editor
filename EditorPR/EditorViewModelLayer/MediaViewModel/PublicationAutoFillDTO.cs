using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class PublicationAutoFillDTO
    {
        public decimal AdValue { get; set; }   // CmPrice
        public decimal PrValue { get; set; }   // AdValue * 3.5
        public string? MediaType { get; set; }
        public string? MediaTier { get; set; }   // from PublicationCustomerCategory
        public string? Frequency { get; set; }
        public string? Language { get; set; }
        public int? Circulation { get; set; }
        public int Reach { get; set; }   // Circulation * 4
    }
}
