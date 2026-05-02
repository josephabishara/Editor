using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class AssistantRepository : GenericRepository<Assistant>, IAssistantRepository
    {
        public AssistantRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Assistant>> GetByClientIdAsync(int clientId)
            => await _dbSet
                .Where(a => a.ClientId == clientId && a.IsActive && a.Deleted == 0)
                .OrderBy(a => a.Name)
                .ToListAsync();

        public async Task<bool> EmailExistsAsync(string email, int excludeId = 0)
            => await _dbSet.AnyAsync(a => a.Email == email && a.Id != excludeId);

        public async Task<bool> UsernameExistsAsync(string username, int excludeId = 0)
            => await _dbSet.AnyAsync(a => a.Username == username && a.Id != excludeId);
    }
}
