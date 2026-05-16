using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class PublicationCustomerCategoryRepository : IPublicationCustomerCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public PublicationCustomerCategoryRepository(ApplicationDbContext context)
            => _context = context;

        public async Task<IEnumerable<PublicationCustomerCategory>> GetByClientIdAsync(int clientId)
            => await _context.PublicationCustomerCategories
                .Include(p => p.Publication)
                .Where(p => p.CustomerId == clientId)
                .OrderBy(p => p.Publication.PublicationName)
                .ToListAsync();

        public async Task<PublicationCustomerCategory?> GetByIdAsync(int id)
            => await _context.PublicationCustomerCategories
                .Include(p => p.Publication)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddRangeAsync(IEnumerable<PublicationCustomerCategory> entities)
        {
            await _context.PublicationCustomerCategories.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PublicationCustomerCategory entity)
        {
            _context.PublicationCustomerCategories.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<PublicationCustomerCategory> entities)
        {
            _context.PublicationCustomerCategories.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsForClientAsync(int clientId)
            => await _context.PublicationCustomerCategories.AnyAsync(p => p.CustomerId == clientId);
    }
}
