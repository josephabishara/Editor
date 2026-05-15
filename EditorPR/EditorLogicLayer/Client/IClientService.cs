using EditorViewModelLayer.ClientViewModel;
using EditorViewModelLayer.General;

namespace EditorLogicLayer.Client
{
    public interface IClientService
    {
        // ── Client CRUD ────────────────────────────────────────────────────────
        Task<IEnumerable<ClientDTO>> GetAllAsync();
        Task<ClientDTO?> GetByIdAsync(int id);
        Task<ClientDTO?> GetWithAssistantsAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(ClientDTO model);
        Task<(bool Success, string Message)> UpdateAsync(ClientDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);

        // ── Photo ──────────────────────────────────────────────────────────────
        Task<(bool Success, string Message)> ChangePhotoAsync(int clientId, UploadFileDTO photo);
        Task<(bool Success, string Message)> ChangeAssistantPhotoAsync(int assistantId, UploadFileDTO photo);

        // ── Assistant CRUD ─────────────────────────────────────────────────────

        Task<IEnumerable<AssistantDTO>> GetAssistantsByClientAsync(int clientId);
        Task<AssistantDTO?> GetAssistantByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAssistantAsync(AssistantDTO model);
        Task<(bool Success, string Message)> UpdateAssistantAsync(AssistantDTO model);
        Task<(bool Success, string Message)> DeleteAssistantAsync(int id);


        // ── Website Categories ─────────────────────────────────────────────────
        Task<IEnumerable<WebsiteCustomerCategoryDTO>> GetClientCategoriesAsync(int clientId);
        Task<(bool Success, string Message)> UpdateClientCategoriesAsync(UpdateClientCategoriesDTO model);


    }
}
