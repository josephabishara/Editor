using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class NewsRepository : GenericRepository<News>, INewsRepository
    {
        public NewsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<News>> GetActiveNewsAsync()
            => await _dbSet
                .Where(n => n.IsActive && n.Deleted == 0)
                .OrderByDescending(n => n.Date)
                .ToListAsync();

        public async Task<IEnumerable<News>> GetBySourceTypeAsync(string sourceType)
            => await _dbSet
                .Where(n => n.SourceType == sourceType && n.IsActive && n.Deleted == 0)
                .OrderByDescending(n => n.Date)
                .ToListAsync();
    }
}
