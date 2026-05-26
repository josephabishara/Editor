using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ClientVideoRepository
        : GenericRepository<ClientVideo>, IClientVideoRepository
    {
        public ClientVideoRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClientVideo>> GetByClientIdAsync(int clientId)
            => await _dbSet
                .Where(v => v.ClientId == clientId && v.IsActive && v.Deleted == 0)
                .OrderByDescending(v => v.Date)
                .ToListAsync();

        public async Task<ClientVideo?> GetByIdWithDetailsAsync(int id)
            => await _dbSet.FirstOrDefaultAsync(v => v.Id == id);
    }
}
