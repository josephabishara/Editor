using EditorViewModelLayer.MediaViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.ClientVideoLogic
{
    public interface IClientVideoService
    {
        Task<ClientVideoListDTO> GetListAsync(int clientId);
        Task<ClientVideoDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(ClientVideoDTO model, IFormFileCollection files);
        Task<(bool Success, string Message)> UpdateAsync(ClientVideoDTO model, IFormFileCollection files);
        Task<(bool Success, string Message)> DeleteAsync(int id);

        // Dropdown builders
        Task<List<MediaSelectOption>> GetChannelOptionsAsync(int selectedId = 0);
        Task<List<MediaSelectOption>> GetCategoryOptionsAsync(int clientId, int selectedId = 0);
        Task<List<MediaSelectOption>> GetSubCategoryOptionsAsync(int parentId, int selectedId = 0);

        // AJAX: auto-fill channel fields
        Task<ChannelAutoFillDTO> GetChannelAutoFillAsync(int channelId, int clientId);
    }
}
