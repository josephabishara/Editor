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
                .Where(a => a.IsActive && a.Deleted == 0)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
    }
}
