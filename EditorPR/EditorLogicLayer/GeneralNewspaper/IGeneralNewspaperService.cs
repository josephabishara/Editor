using EditorViewModelLayer.GeneralNewspaperViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.GeneralNewspaper
{
    public interface IGeneralNewspaperService
    {
        Task<IEnumerable<GeneralNewspaperDTO>> GetAllAsync();
        Task<GeneralNewspaperDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(GeneralNewspaperDTO model);
        Task<(bool Success, string Message)> UpdateAsync(GeneralNewspaperDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);

        // ── Filtering (Index) ─────────────────────────────────────────────────
        Task<IEnumerable<GeneralNewspaperDTO>> GetFilteredAsync(GeneralNewspaperFilterDTO filter);
        Task<GeneralNewspaperIndexVM> GetIndexViewModelAsync(GeneralNewspaperFilterDTO filter);

        // ── Excel ──────────────────────────────────────────────────────────
        byte[] ExportToExcel(IEnumerable<GeneralNewspaperDTO> newspapers);
        Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(Stream fileStream, string fileName);

        // ── Dropdown builders (Create/Edit views) ───────────────────────────
        Task<List<EditorViewModelLayer.MediaViewModel.MediaSelectOption>> GetPublicationOptionsAsync(int selectedId = 0);
        Task<List<EditorViewModelLayer.MediaViewModel.MediaSelectOption>> GetWriterOptionsAsync(int selectedId = 0);

    }
}
