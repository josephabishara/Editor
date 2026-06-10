using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace EditorRepositoryLayer.Repositories
{
    public class ClientCategoryRepository
        : GenericRepository<ClientCategories>, IClientCategoryRepository
    {
        public ClientCategoryRepository(ApplicationDbContext context) : base(context) { }

        // Used by Index — returns ALL non-deleted root categories (active + inactive)
        // so the Enable button is reachable for disabled rows
        public async Task<IEnumerable<ClientCategories>> GetRootCategoriesByClientAsync(int clientId)
            => await _dbSet
                .Include(c => c.Children)
                .Where(c => c.ClientId == clientId
                         && c.ParentCategory == null
                         && c.Deleted == 0)          // NOT filtered by IsActive
                .OrderBy(c => c.Order)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

        // Used by dropdowns — only active categories are valid parent choices
        public async Task<IEnumerable<ClientCategories>> GetByClientAsync(int clientId)
            => await _dbSet
                .Include(c => c.Children)
                .Where(c => c.ClientId == clientId
                         && c.IsActive
                         && c.Deleted == 0)
                .OrderBy(c => c.Order)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

        public async Task<IEnumerable<ClientCategories>> GetChildrenAsync(int parentId)
            => await _dbSet
                .Where(c => c.ParentCategory == parentId
                         && c.IsActive
                         && c.Deleted == 0)
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