using EditorViewModelLayer.GeneralArticleViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.GeneralArticle
{
    public interface IGeneralArticleService
    {
        Task<IEnumerable<GeneralArticleDTO>> GetAllAsync();
        Task<GeneralArticleDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(GeneralArticleDTO model);
        Task<(bool Success, string Message)> UpdateAsync(GeneralArticleDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);

        // ── Filtering (Index) ─────────────────────────────────────────────────
        Task<IEnumerable<GeneralArticleDTO>> GetFilteredAsync(GeneralArticleFilterDTO filter);
        Task<GeneralArticleIndexVM> GetIndexViewModelAsync(GeneralArticleFilterDTO filter);

        // ── Excel ──────────────────────────────────────────────────────────
        byte[] ExportToExcel(IEnumerable<GeneralArticleDTO> articles);
        Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(Stream fileStream, string fileName);
    }
}
