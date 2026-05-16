using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ClientNewsRepository : GenericRepository<ClientNews>, IClientNewsRepository
    {
        public ClientNewsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClientNews>> GetByClientIdAsync(int clientId)
            => await _dbSet
                .Include(cn => cn.News)
                .Include(cn => cn.Client)
                .Include(cn => cn.Writer)
                .Where(cn => cn.ClientId == clientId && cn.IsActive && cn.Deleted == 0)
                .OrderByDescending(cn => cn.Date)
                .ToListAsync();

        public async Task<ClientNews?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(cn => cn.News)
                .Include(cn => cn.Client)
                .Include(cn => cn.Writer)
                .FirstOrDefaultAsync(cn => cn.Id == id);

        public async Task<IEnumerable<ClientNews>> GetByClientAndSourceTypeAsync(
            int clientId, string sourceType)
            => await _dbSet
                .Include(cn => cn.News)
                .Include(cn => cn.Writer)
                .Where(cn => cn.ClientId == clientId
                          && cn.News.SourceType == sourceType
                          && cn.IsActive && cn.Deleted == 0)
                .OrderByDescending(cn => cn.Date)
                .ToListAsync();

        public async Task<IEnumerable<ClientNews>> GetByNewsIdAsync(int newsId)
            => await _dbSet
                .Include(cn => cn.Client)
                .Where(cn => cn.NewsId == newsId && cn.IsActive && cn.Deleted == 0)
                .ToListAsync();
    }
}
