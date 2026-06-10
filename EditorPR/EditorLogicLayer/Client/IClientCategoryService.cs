using EditorViewModelLayer.ClientViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Client
{
    public interface IClientCategoryService
    {
        Task<IEnumerable<ClientCategoryDTO>> GetByClientAsync(int clientId);
        Task<IEnumerable<ClientCategoryDTO>> GetRootCategoriesAsync(int clientId);
        Task<IEnumerable<ClientCategoryDTO>> GetChildrenAsync(int parentId);
        Task<ClientCategoryDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(ClientCategoryDTO model);
        Task<(bool Success, string Message)> UpdateAsync(ClientCategoryDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);

        Task<(bool Success, string Message)> ToggleStatusAsync(int id);

    }
}
