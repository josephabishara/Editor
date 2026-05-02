using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class WebsiteRepository : GenericRepository<Websites>, IWebsiteRepository
    {
        public WebsiteRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Websites>> GetActiveWebsitesAsync()
            => await _dbSet.Where(w => w.IsActive).OrderBy(w => w.WebsiteName).ToListAsync();

        public async Task<IEnumerable<Websites>> SearchByNameAsync(string name)
            => await _dbSet
                .Where(w => w.WebsiteName.Contains(name) && w.IsActive)
                .ToListAsync();
    }
}
