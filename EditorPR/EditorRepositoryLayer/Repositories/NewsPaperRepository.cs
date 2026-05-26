using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class NewsPaperRepository : GenericRepository<NewsPaper>, INewsPaperRepository
    {
        public NewsPaperRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<NewsPaper>> GetActiveAsync()
            => await _dbSet
                .Where(n => n.IsActive && n.Deleted == 0)
                .OrderByDescending(n => n.Date)
                .ToListAsync();
    }
}
