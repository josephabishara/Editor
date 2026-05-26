using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    public class ClientVideoDTO
    {
        public int Id { get; set; }
        public int VideoId { get; set; }  // reference → GeneralVideos.Id

        [Required]
        public int ClientId { get; set; }
        public string? ClientName { get; set; }

        // ── Source ────────────────────────────────────────────────────────────
        [Required]
        [Display(Name = "Channel")]
        public int ChannelId { get; set; }
        public string? ChannelName { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        // ── Auto-filled from Channel ──────────────────────────────────────────
        [Display(Name = "Media Tier")]
        public string? MediaTier { get; set; }  // from ChannelCustomerCategory

        [Display(Name = "Impression")]
        public int Impression { get; set; }

        [Display(Name = "Reach")]
        public int Reach { get; set; }   // Impression * 4

        [Display(Name = "AD Value")]
        public decimal ADValue { get; set; }   // UnitPrice from Channel

        [Display(Name = "PR Value")]
        public decimal PRValue { get; set; }   // ADValue * 3.5

        // ── Video fields ──────────────────────────────────────────────────────
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [MaxLength(500)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Program")]
        public string? Program { get; set; }

        [Display(Name = "Duration (seconds)")]
        [Range(0, int.MaxValue)]
        public int Duration { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Toning")]
        public string? Toning { get; set; }

        [Display(Name = "Video URL")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Video File")]
        public string? VideoFile { get; set; }   // saved path

        [Display(Name = "Screenshot")]
        public string? Screenshot { get; set; }   // saved path

        // ── Dropdown lists ────────────────────────────────────────────────────
        public List<MediaSelectOption> ChannelOptions { get; set; } = new();
        public List<MediaSelectOption> CategoryOptions { get; set; } = new();
        public List<MediaSelectOption> SubCategoryOptions { get; set; } = new();
        public List<MediaSelectOption> ToningOptions { get; set; } = new();
    }
}
