using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ClientArticleRepository
       : GenericRepository<ClientArticle>, IClientArticleRepository
    {
        public ClientArticleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClientArticle>> GetByClientIdAsync(int clientId)
            => await _dbSet
                .Where(a => a.ClientId == clientId && a.IsActive && a.Deleted == 0)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

        public async Task<ClientArticle?> GetByIdWithDetailsAsync(int id)
            => await _dbSet.FirstOrDefaultAsync(a => a.Id == id);
    }
}
