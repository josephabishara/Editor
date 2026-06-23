using EditorViewModelLayer.MediaViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.GeneralArticleViewModel
{
    public class GeneralArticleIndexVM
    {
        public IEnumerable<GeneralArticleDTO> Items { get; set; } = new List<GeneralArticleDTO>();
        public GeneralArticleFilterDTO Filter { get; set; } = new();
        public List<MediaSelectOption> Websites { get; set; } = new();
    }
}
