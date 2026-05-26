using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class ChannelAutoFillDTO
    {
        public decimal AdValue { get; set; }   // UnitPrice
        public decimal PrValue { get; set; }   // AdValue * 3.5
        public string? MediaTier { get; set; }   // from ChannelCustomerCategory
        public int Impression { get; set; }
        public int Reach { get; set; }   // Impression * 4
    }
}
