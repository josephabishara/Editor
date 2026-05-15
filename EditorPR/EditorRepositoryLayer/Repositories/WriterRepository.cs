using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class WriterRepository : GenericRepository<Writer>, IWriterRepository
    {
        public WriterRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Writer>> GetActiveWritersAsync()
            => await _dbSet.Where(w => w.IsActive).OrderBy(w => w.WriterName).ToListAsync();

        public async Task<IEnumerable<Writer>> SearchByNameAsync(string name)
            => await _dbSet
                .Where(w => w.WriterName.Contains(name) && w.IsActive)
                .ToListAsync();
    }
}
