using EditorViewModelLayer.NewsViewModel;
using EditorViewModelLayer.ReportViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Reports
{
    public interface IReportService
    {
        // ── CRUD ────────────────────────────────────────────────────────────────
        Task<IEnumerable<ReportDTO>> GetAllAsync();
        Task<ReportDTO?> GetByIdAsync(int id);
        Task<ReportDetailsDTO?> GetDetailsAsync(int id);
        Task<(bool Success, string Message, int ReportId)> CreateAsync(ReportDTO model);
        Task<(bool Success, string Message)> UpdateAsync(ReportDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);

        // ── Publish / UnPublish ─────────────────────────────────────────────────
        Task<(bool Success, string Message)> PublishAsync(int id);
        Task<(bool Success, string Message)> UnPublishAsync(int id);

        // ── Wizard steps ────────────────────────────────────────────────────────
        Task<List<ReportArticlePickerDTO>> GetArticlePickerAsync(int reportId, DateTime? from, DateTime? to);
        Task<(bool Success, string Message)> SaveArticlesAsync(int reportId, List<int> articleIds);

        Task<List<ReportNewspaperPickerDTO>> GetNewspaperPickerAsync(int reportId, DateTime? from, DateTime? to);
        Task<(bool Success, string Message)> SaveNewspapersAsync(int reportId, List<int> newspaperIds);

        // ── Dropdowns ───────────────────────────────────────────────────────────
        Task<IEnumerable<SelectOption>> GetClientOptionsAsync();
    }
}
