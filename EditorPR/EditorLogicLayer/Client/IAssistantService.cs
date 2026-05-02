using EditorViewModelLayer.ClientViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Client
{
    public interface IAssistantService
    {
        // Assistant CRUD
        Task<IEnumerable<AssistantDTO>> GetAssistantsByClientAsync(int clientId);
        Task<AssistantDTO?> GetAssistantByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAssistantAsync(AssistantDTO model);
        Task<(bool Success, string Message)> UpdateAssistantAsync(AssistantDTO model);
        Task<(bool Success, string Message)> DeleteAssistantAsync(int id);
    }
}
