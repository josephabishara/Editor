using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ChannelCustomerCategoryRepository : IChannelCustomerCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ChannelCustomerCategoryRepository(ApplicationDbContext context)
            => _context = context;

        public async Task<IEnumerable<ChannelCustomerCategory>> GetByClientIdAsync(int clientId)
            => await _context.ChannelCustomerCategories
                .Include(c => c.Channel)
                .Where(c => c.CustomerId == clientId)
                .OrderBy(c => c.Channel.ChannelName)
                .ToListAsync();

        public async Task<ChannelCustomerCategory?> GetByIdAsync(int id)
            => await _context.ChannelCustomerCategories
                .Include(c => c.Channel)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddRangeAsync(IEnumerable<ChannelCustomerCategory> entities)
        {
            await _context.ChannelCustomerCategories.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ChannelCustomerCategory entity)
        {
            _context.ChannelCustomerCategories.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<ChannelCustomerCategory> entities)
        {
            _context.ChannelCustomerCategories.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsForClientAsync(int clientId)
            => await _context.ChannelCustomerCategories.AnyAsync(c => c.CustomerId == clientId);
    }
}
