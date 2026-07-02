using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Report>> GetActiveAsync()
            => await _dbSet
                .Include(r => r.Customer)
                .Where(r => r.IsActive && r.Deleted == 0)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();

        public async Task<Report?> GetByIdWithNavAsync(int id)
     => await _dbSet
         .Include(r => r.Customer)
         .Include(r => r.ReportArticles)
             .ThenInclude(ra => ra.Article)
         .Include(r => r.ReportNewspapers)
             .ThenInclude(rn => rn.NewsPaper)
         .FirstOrDefaultAsync(r => r.Id == id && r.IsActive && r.Deleted == 0);

        public async Task<IEnumerable<Report>> GetByClientIdAsync(int clientId)
            => await _dbSet
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == clientId && r.IsActive && r.Deleted == 0)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
    }
}
