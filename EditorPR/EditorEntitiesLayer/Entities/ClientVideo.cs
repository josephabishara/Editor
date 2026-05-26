using System;
using System.Collections.Generic;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class ClientVideo : BaseEntity
    {
        public int VideoId { get; set; }// reference  not FK

        public int ClientId { get; set; } // FK → Client and Required
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public int CategoryId { get; set; }      // FK → ClientCategories
        public int SubCategoryId { get; set; }   // FK → ClientCategories (child)

        public string? description { get; set; } = null;

        public int ChannelId { get; set; } // FK → Channel and Required
        public string VideoUrl { get; set; }
        public string VideoFileFile { get; set; }
        public string ScreenshotFile { get; set; }
      
        // append to ClientVideo.cs
        public decimal? ADValue { get; set; }
        public decimal? PRValue { get; set; }   // = ADValue * 3.5

        // Duration
        public decimal Duration { get; set; }
        public string? Toning { get; set; } // None, Positive,  Neutral, Negative


    }
}
