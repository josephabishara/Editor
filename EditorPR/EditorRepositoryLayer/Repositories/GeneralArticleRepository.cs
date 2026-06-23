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

        // ── Filtered query ─────────────────────────────────────────────────────
        public async Task<IEnumerable<GeneralArticle>> GetFilteredAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? title,
            int? websiteId)
        {
            var query = _dbSet
                .Include(a => a.Website)
                .Include(a => a.Writer)
                .Where(a => a.IsActive && a.Deleted == 0);

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(a => a.Date >= from);
            }

            if (toDate.HasValue)
            {
                // Inclusive of the entire "to" day
                var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.Date <= to);
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                var term = title.Trim();
                query = query.Where(a => a.Title != null && a.Title.Contains(term));
            }

            if (websiteId.HasValue && websiteId.Value > 0)
            {
                query = query.Where(a => a.WebsiteId == websiteId.Value);
            }

            return await query
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }
    }
}
