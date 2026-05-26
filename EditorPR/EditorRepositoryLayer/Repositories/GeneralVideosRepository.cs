using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class GeneralVideosRepository
      : GenericRepository<GeneralVideos>, IGeneralVideosRepository
    {
        public GeneralVideosRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<GeneralVideos>> GetActiveAsync()
            => await _dbSet
                .Where(v => v.IsActive && v.Deleted == 0)
                .OrderByDescending(v => v.Date)
                .ToListAsync();
    }
}
