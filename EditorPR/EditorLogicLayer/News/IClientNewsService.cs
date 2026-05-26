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

        // ── Dropdown builders — return SelectOption (no MVC dependency) ────────
        Task<List<SelectOption>> GetSourceOptionsAsync(string sourceType, int selectedId = 0);
        Task<List<SelectOption>> GetCategoryOptionsAsync(int clientId, int selectedId = 0);
        Task<List<SelectOption>> GetSubCategoryOptionsAsync(int parentId, int selectedId = 0);
        Task<List<SelectOption>> GetWriterOptionsAsync(int selectedId = 0);
        Task<List<SelectOption>> GetExistingNewsOptionsAsync(string sourceType, int selectedId = 0);

        // ── AJAX prefill ───────────────────────────────────────────────────────
        Task<ClientNewsDTO?> PrefillFromExistingNewsAsync(int newsId, int clientId);
    }
}
