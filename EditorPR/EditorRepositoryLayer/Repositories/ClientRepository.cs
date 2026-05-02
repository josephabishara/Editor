using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        public ClientRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Client>> GetActiveClientsAsync()
            => await _dbSet
                .Where(c => c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task<Client?> GetClientWithAssistantsAsync(int id)
            => await _dbSet
                .Include(c => c.AssistantList)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<bool> EmailExistsAsync(string email, int excludeId = 0)
            => await _dbSet.AnyAsync(c => c.Email == email && c.Id != excludeId);

        public async Task<bool> UsernameExistsAsync(string username, int excludeId = 0)
            => await _dbSet.AnyAsync(c => c.Username == username && c.Id != excludeId);
    }

   
}
