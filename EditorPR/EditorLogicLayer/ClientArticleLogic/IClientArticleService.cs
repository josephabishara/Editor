using EditorViewModelLayer.MediaViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.ClientArticleLogic
{
    public interface IClientArticleService
    {
        Task<ClientArticleListDTO> GetListAsync(int clientId);
        Task<ClientArticleDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(ClientArticleDTO model);
        Task<(bool Success, string Message)> UpdateAsync(ClientArticleDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);

        // Dropdown builders
        Task<List<MediaSelectOption>> GetWebsiteOptionsAsync(int selectedId = 0);
        Task<List<MediaSelectOption>> GetCategoryOptionsAsync(int clientId, int selectedId = 0);
        Task<List<MediaSelectOption>> GetSubCategoryOptionsAsync(int parentId, int selectedId = 0);
        Task<List<MediaSelectOption>> GetWriterOptionsAsync(int selectedId = 0);

        // AJAX: auto-fill website fields by websiteId + clientId
        Task<WebsiteAutoFillDTO> GetWebsiteAutoFillAsync(int websiteId, int clientId);
    }
}
