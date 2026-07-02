using EditorViewModelLayer.GeneralNewspaperViewModel;
using EditorViewModelLayer.MediaViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.ClientNewsPaperLogic
{
    public interface IClientNewsPaperService
    {
        Task<ClientNewsPaperListDTO> GetListAsync(int clientId);
        Task<ClientNewsPaperDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int NewId)> CreateAsync(ClientNewsPaperDTO model);
        Task<(bool Success, string Message)> UpdateAsync(ClientNewsPaperDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<(bool Success, string Message)> PublishAsync(int id);
        Task<(bool Success, string Message)> UnpublishAsync(int id);
        Task<(bool Success, string Message)> BulkDeleteAsync(IEnumerable<int> ids);    
        // Dropdown builders
        Task<List<MediaSelectOption>> GetPublicationOptionsAsync(int selectedId = 0);
        Task<List<MediaSelectOption>> GetCategoryOptionsAsync(int clientId, int selectedId = 0);
        Task<List<MediaSelectOption>> GetSubCategoryOptionsAsync(int parentId, int selectedId = 0);
        Task<List<MediaSelectOption>> GetWriterOptionsAsync(int selectedId = 0);

        // AJAX: auto-fill publication fields by publicationId + clientId
        Task<PublicationAutoFillDTO> GetPublicationAutoFillAsync(int publicationId, int clientId);
        Task<(bool Success, string Message, int NewId)> DuplicateAsync(int id);

        Task<List<ShareNewspaperClientOptionDTO>> GetShareClientOptionsAsync(int generalNewspaperId);
        Task<(bool Success, string Message, int CreatedCount)> ShareToClientsAsync(ShareNewspaperToClientsDTO model);




    }
}
