using System;
using System.Collections.Generic;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class GeneralVideos : BaseEntity
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public int ChannelId { get; set; }
        public string? description { get; set; } = null;

        public string VideoUrl { get; set; }
        public string VideoFileFile { get; set; }
        public string ScreenshotFile { get; set; }

        // Duration
        public decimal Duration { get; set; }
        public string? Toning { get; set; } // None, Positive,  Neutral, Negative

    }
}
