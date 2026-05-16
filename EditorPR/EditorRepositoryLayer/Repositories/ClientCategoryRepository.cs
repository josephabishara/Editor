using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ClientCategoryRepository : GenericRepository<ClientCategories>, IClientCategoryRepository
    {
        public ClientCategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClientCategories>> GetByClientAsync(int clientId)
            => await _dbSet
                .Include(c => c.Children)
               // .Include(c => c.Articles)
                .Where(c => c.ClientId == clientId && c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.Order)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

        public async Task<IEnumerable<ClientCategories>> GetRootCategoriesByClientAsync(int clientId)
            => await _dbSet
                .Include(c => c.Children)
               // .Include(c => c.Articles)
                .Where(c => c.ClientId == clientId
                         && c.ParentCategory == null
                         && c.IsActive
                         && c.Deleted == 0)
                .OrderBy(c => c.Order)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

        public async Task<IEnumerable<ClientCategories>> GetChildrenAsync(int parentId)
            => await _dbSet
              //  .Include(c => c.Articles)
                .Where(c => c.ParentCategory == parentId && c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.Order)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

        public async Task<bool> CategoryNameExistsAsync(int clientId, string name, int? excludeId = null)
            => await _dbSet.AnyAsync(c =>
                c.ClientId == clientId &&
                c.CategoryName == name &&
                c.IsActive &&
                c.Deleted == 0 &&
                (excludeId == null || c.Id != excludeId));
    }
}
