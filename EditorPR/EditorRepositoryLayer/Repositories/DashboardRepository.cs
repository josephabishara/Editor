using EditorDataLayer.Data;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
            => _context = context;

        // ── Articles ──────────────────────────────────────────────────────

        public Task<int> GetArticleCountAsync(int clientId)
            => _context.Set<EditorEntitiesLayer.Entities.ClientArticle>()
                .CountAsync(a => a.ClientId == clientId
                              && a.IsActive
                              && a.Deleted == 0);

        public Task<decimal> GetArticleTotalPRAsync(int clientId)
            => _context.Set<EditorEntitiesLayer.Entities.ClientArticle>()
                .Where(a => a.ClientId == clientId
                         && a.IsActive
                         && a.Deleted == 0)
                .SumAsync(a => a.PRValue ?? 0m);

        // ── Newspapers ────────────────────────────────────────────────────

        public Task<int> GetNewsPaperCountAsync(int clientId)
            => _context.Set<EditorEntitiesLayer.Entities.ClientNewsPaper>()
                .CountAsync(n => n.ClientId == clientId
                              && n.IsActive
                              && n.Deleted == 0);

        public Task<decimal> GetNewsPaperTotalPRAsync(int clientId)
            => _context.Set<EditorEntitiesLayer.Entities.ClientNewsPaper>()
                .Where(n => n.ClientId == clientId
                         && n.IsActive
                         && n.Deleted == 0)
                .SumAsync(n => n.PRValue);

        // ── Videos ───────────────────────────────────────────────────────

        public Task<int> GetVideoCountAsync(int clientId)
            => _context.Set<EditorEntitiesLayer.Entities.ClientVideo>()
                .CountAsync(v => v.ClientId == clientId
                              && v.IsActive
                              && v.Deleted == 0);

        public Task<decimal> GetVideoTotalADAsync(int clientId)
            => _context.Set<EditorEntitiesLayer.Entities.ClientVideo>()
                .Where(v => v.ClientId == clientId
                         && v.IsActive
                         && v.Deleted == 0)
                .SumAsync(v => (decimal?)v.ADValue ?? 0m);  // guard if field is nullable

        public Task<decimal> GetVideoTotalPRAsync(int clientId)
            => _context.Set<EditorEntitiesLayer.Entities.ClientVideo>()
                .Where(v => v.ClientId == clientId
                         && v.IsActive
                         && v.Deleted == 0)
                .SumAsync(v => (decimal?)v.PRValue ?? 0m);
    }
}
