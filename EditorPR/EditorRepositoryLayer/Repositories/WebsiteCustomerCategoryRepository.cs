using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class WebsiteCustomerCategoryRepository : IWebsiteCustomerCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public WebsiteCustomerCategoryRepository(ApplicationDbContext context)
            => _context = context;

        public async Task<IEnumerable<WebsiteCustomerCategory>> GetByClientIdAsync(int clientId)
            => await _context.WebsiteCustomerCategories
                .Include(w => w.Website)
                .Where(w => w.CustomerId == clientId)
                .OrderBy(w => w.Website.WebsiteName)
                .ToListAsync();

        public async Task<WebsiteCustomerCategory?> GetByIdAsync(int id)
            => await _context.WebsiteCustomerCategories
                .Include(w => w.Website)
                .FirstOrDefaultAsync(w => w.Id == id);

        public async Task AddRangeAsync(IEnumerable<WebsiteCustomerCategory> entities)
        {
            await _context.WebsiteCustomerCategories.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WebsiteCustomerCategory entity)
        {
            _context.WebsiteCustomerCategories.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<WebsiteCustomerCategory> entities)
        {
            _context.WebsiteCustomerCategories.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsForClientAsync(int clientId)
            => await _context.WebsiteCustomerCategories.AnyAsync(w => w.CustomerId == clientId);
    }
}
