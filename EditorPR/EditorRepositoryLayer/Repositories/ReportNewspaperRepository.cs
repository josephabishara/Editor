using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ReportNewspaperRepository : GenericRepository<ReportNewspaper>, IReportNewspaperRepository
    {
        public ReportNewspaperRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ReportNewspaper>> GetByReportIdAsync(int reportId)
            => await _dbSet
                .Include(rn => rn.NewsPaper)
                .Where(rn => rn.ReportId == reportId)
                .ToListAsync();

        public async Task DeleteByReportIdAsync(int reportId)
        {
            var rows = await _dbSet.Where(rn => rn.ReportId == reportId).ToListAsync();
            if (rows.Any())
            {
                _dbSet.RemoveRange(rows);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<ReportNewspaper> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}
