using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace EditorRepositoryLayer.Repositories
{
    public class ReportArticleRepository : GenericRepository<ReportArticle>, IReportArticleRepository
    {
        public ReportArticleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ReportArticle>> GetByReportIdAsync(int reportId)
     => await _dbSet
         .Include(ra => ra.Article)
         .Where(ra => ra.ReportId == reportId)
         .ToListAsync();

        public async Task DeleteByReportIdAsync(int reportId)
        {
            var rows = await _dbSet.Where(ra => ra.ReportId == reportId).ToListAsync();
            if (rows.Any())
            {
                _dbSet.RemoveRange(rows);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<ReportArticle> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}
