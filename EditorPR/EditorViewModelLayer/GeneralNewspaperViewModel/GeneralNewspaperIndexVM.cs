using EditorViewModelLayer.MediaViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorViewModelLayer.GeneralNewspaperViewModel
{
    // ── Composite VM returned to the Index view (results + filter + dropdown) ─
    public class GeneralNewspaperIndexVM
    {
        public IEnumerable<GeneralNewspaperDTO> Items { get; set; } = new List<GeneralNewspaperDTO>();
        public GeneralNewspaperFilterDTO Filter { get; set; } = new();
        public List<MediaSelectOption> Publications { get; set; } = new();
    }
}
