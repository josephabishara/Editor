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
        byte[] ExportWebsiteCategoriesToExcel(IEnumerable<WebsiteCustomerCategoryDTO> categories, string clientName);
        Task<(bool Success, string Message, int UpdatedCount)> ImportWebsiteCategoriesFromExcelAsync(int clientId, Stream fileStream, string fileName);

        // ── Publication Categories ─────────────────────────────────────────────
        Task<IEnumerable<PublicationCustomerCategoryDTO>> GetClientPublicationCategoriesAsync(int clientId);
        Task<(bool Success, string Message)> UpdateClientPublicationCategoriesAsync(UpdateClientPublicationCategoriesDTO model);
        byte[] ExportPublicationCategoriesToExcel(IEnumerable<PublicationCustomerCategoryDTO> categories, string clientName);
        Task<(bool Success, string Message, int UpdatedCount)> ImportPublicationCategoriesFromExcelAsync(int clientId, Stream fileStream, string fileName);

        // ── Channel Categories ─────────────────────────────────────────────────
        Task<IEnumerable<ChannelCustomerCategoryDTO>> GetClientChannelCategoriesAsync(int clientId);
        Task<(bool Success, string Message)> UpdateClientChannelCategoriesAsync(UpdateClientChannelCategoriesDTO model);
        byte[] ExportChannelCategoriesToExcel(IEnumerable<ChannelCustomerCategoryDTO> categories, string clientName);
        Task<(bool Success, string Message, int UpdatedCount)> ImportChannelCategoriesFromExcelAsync(int clientId, Stream fileStream, string fileName);

        // ── Report Cover ────────────────────────────────────────────────────────

        Task<(bool Success, string Message)> ChangeReportCoverAsync(int clientId, UploadFileDTO file);


    }
}
