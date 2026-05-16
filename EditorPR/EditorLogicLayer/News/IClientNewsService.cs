using EditorViewModelLayer.NewsViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.News
{
    public interface IClientNewsService
    {
        // ── List ───────────────────────────────────────────────────────────────
        Task<ClientNewsListDTO> GetClientNewsDashboardAsync(int clientId);
        Task<IEnumerable<ClientNewsDTO>> GetByClientIdAsync(int clientId);

        // ── Single ─────────────────────────────────────────────────────────────
        Task<ClientNewsDTO?> GetByIdAsync(int clientNewsId);

        // ── Create / Update / Delete ───────────────────────────────────────────
        Task<(bool Success, string Message)> CreateAsync(ClientNewsDTO model);
        Task<(bool Success, string Message)> UpdateAsync(ClientNewsDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int clientNewsId);

        // ── Publish ────────────────────────────────────────────────────────────
        Task<(bool Success, string Message)> PublishAsync(int clientNewsId);
        Task<(bool Success, string Message)> UnpublishAsync(int clientNewsId);

        // ── SelectListItem builders (used by controller to populate DTO) ───────
        Task<List<SelectListItem>> GetSourceSelectListAsync(string sourceType, int selectedId = 0);
        Task<List<SelectListItem>> GetCategorySelectListAsync(int clientId, int selectedId = 0);
        Task<List<SelectListItem>> GetSubCategorySelectListAsync(int parentId, int selectedId = 0);
        Task<List<SelectListItem>> GetWriterSelectListAsync(int selectedId = 0);
        Task<List<SelectListItem>> GetExistingNewsSelectListAsync(string sourceType, int selectedId = 0);

        // ── AJAX prefill ───────────────────────────────────────────────────────
        Task<ClientNewsDTO?> PrefillFromExistingNewsAsync(int newsId, int clientId);
    }
}
