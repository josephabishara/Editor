using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.MediaViewModel
{
    // ── Shared SelectOption (same pattern as NewsViewModel) ───────────────────
    public class MediaSelectOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; } = false;
    }

    public static class MediaOptions
    {
        public static readonly string[] BrandingOpts = { "Branded", "Unbranded" };
        public static readonly string[] ToningOpts = {  "Positive", "Neutral", "Negative" };
        public static readonly string[] YesNoOpts = { "Yes", "No" };
        public static readonly string[] GenerationOpts = { "Generated", "Not Generated" };

        public static List<MediaSelectOption> BrandingList() =>
            BrandingOpts.Select(o => new MediaSelectOption { Value = o, Text = o }).ToList();

        public static List<MediaSelectOption> ToningList() =>
            ToningOpts.Select(o => new MediaSelectOption { Value = o, Text = o }).ToList();
        public static List<MediaSelectOption> YesNoList() =>
            YesNoOpts.Select(o => new MediaSelectOption { Value = o, Text = o }).ToList();
        public static List<MediaSelectOption> GenerationList() =>
            GenerationOpts.Select(o => new MediaSelectOption { Value = o, Text = o }).ToList();
    }

    
}
