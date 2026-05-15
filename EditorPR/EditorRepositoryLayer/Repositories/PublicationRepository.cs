using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class PublicationRepository : GenericRepository<Publication>, IPublicationRepository
    {
        public PublicationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Publication>> GetActivePublicationsAsync()
            => await _dbSet
                .Where(p => p.IsActive && p.Deleted == 0)
                .OrderBy(p => p.PublicationName)
                .ToListAsync();

        public async Task<IEnumerable<Publication>> SearchByNameAsync(string name)
            => await _dbSet
                .Where(p => p.PublicationName.Contains(name) && p.IsActive && p.Deleted == 0)
                .ToListAsync();
    }
}
