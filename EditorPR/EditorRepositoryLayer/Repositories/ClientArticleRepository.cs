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

        /// <summary>
        /// Returns a single article by id.
        /// The service resolves display names with separate FindAsync calls after this.
        /// </summary>
        public async Task<ClientArticle?> GetByIdWithDetailsAsync(int id)
            => await _dbSet.FirstOrDefaultAsync(a => a.Id == id);

        /// <summary>
        /// Returns all active child rows for a given parent article id.
        /// </summary>
        public async Task<IEnumerable<ClientArticle>> GetChildrenAsync(int parentId)
            => await _dbSet
                .Where(a => a.ParentId == parentId && a.IsActive && a.Deleted == 0)
                .OrderBy(a => a.Date)
                .ToListAsync();
    }
}
