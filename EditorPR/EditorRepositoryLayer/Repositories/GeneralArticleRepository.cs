using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class GeneralArticleRepository
       : GenericRepository<GeneralArticle>, IGeneralArticleRepository
    {
        public GeneralArticleRepository(ApplicationDbContext context) : base(context) { }


        public async Task<IEnumerable<GeneralArticle>> GetActiveAsync()
            => await _dbSet
                .Include(a => a.Website)
                .Include(a => a.Writer)
                .Where(a => a.IsActive && a.Deleted == 0)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        public async Task<GeneralArticle?> GetByIdWithNavAsync(int id)
        => await _dbSet
            .Include(a => a.Website)
            .Include(a => a.Writer)
            .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<GeneralArticle>> GetByIdsAsync(IEnumerable<int> ids)
            => await _dbSet
                .Include(a => a.Website)
                .Include(a => a.Writer)
                .Where(a => ids.Contains(a.Id) && a.IsActive && a.Deleted == 0)
                .ToListAsync();
    }
}
