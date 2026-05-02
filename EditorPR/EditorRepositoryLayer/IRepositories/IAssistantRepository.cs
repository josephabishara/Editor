using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IAssistantRepository : IRepository<Assistant>
    {
        Task<IEnumerable<Assistant>> GetByClientIdAsync(int clientId);
        Task<bool> EmailExistsAsync(string email, int excludeId = 0);
        Task<bool> UsernameExistsAsync(string username, int excludeId = 0);
    }
}
