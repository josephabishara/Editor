using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<IEnumerable<Client>> GetActiveClientsAsync();
        Task<Client?> GetClientWithAssistantsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int excludeId = 0);
        Task<bool> UsernameExistsAsync(string username, int excludeId = 0);
    }

    
}
