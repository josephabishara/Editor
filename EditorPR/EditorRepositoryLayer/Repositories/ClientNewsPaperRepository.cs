using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ClientNewsPaperRepository
       : GenericRepository<ClientNewsPaper>, IClientNewsPaperRepository
    {
        public ClientNewsPaperRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ClientNewsPaper>> GetByClientIdAsync(int clientId)
            => await _dbSet
                .Include(c => c.Client)
                .Include(c => c.Writer)
                .Include(c => c.NewsPaper)
                .Where(c => c.ClientId == clientId && c.IsActive && c.Deleted == 0)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

        public async Task<ClientNewsPaper?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(c => c.Client)
                .Include(c => c.Writer)
                .Include(c => c.NewsPaper)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<ClientNewsPaper>> GetByNewsPaperIdAsync(int newsPaperId)
            => await _dbSet
                .Where(c => c.NewsPaperId == newsPaperId && c.IsActive && c.Deleted == 0)
                .ToListAsync();

        public async Task<IEnumerable<ClientNewsPaper>> GetChildrenAsync(int parentId)
            => await _dbSet
                .Where(n => n.ParentId == parentId && n.IsActive && n.Deleted == 0)
                .OrderBy(n => n.Date)
                .ToListAsync();
    }


}
