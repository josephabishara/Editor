using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
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

        public Task<decimal> GetArticleTotalADAsync(int clientId)
      => _context.Set<EditorEntitiesLayer.Entities.ClientArticle>()
              .Where(a => a.ClientId == clientId
                       && a.IsActive
                       && a.Deleted == 0)
              .SumAsync(a => a.ADValue ?? 0m);

        public Task<int> GetArticleCountAsync(int clientId, DateTime? from, DateTime? to)
          => Articles(clientId, from, to).CountAsync();

        public Task<decimal> GetArticleTotalPRAsync(int clientId, DateTime? from, DateTime? to)
            => Articles(clientId, from, to).SumAsync(a => a.PRValue ?? 0m);

        public Task<decimal> GetArticleTotalADAsync(int clientId, DateTime? from, DateTime? to)
            => Articles(clientId, from, to).SumAsync(a => a.ADValue ?? 0m);




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
                .SumAsync(n => n.PRValue ?? 0m);

        public Task<decimal> GetNewsPaperTotalADAsync(int clientId)
            => _context.Set<EditorEntitiesLayer.Entities.ClientNewsPaper>()
             .Where(n => n.ClientId == clientId
                      && n.IsActive
                      && n.Deleted == 0)
             .SumAsync(n => n.ADValue ?? 0m);

        public Task<int> GetNewsPaperCountAsync(int clientId, DateTime? from, DateTime? to)
          => Papers(clientId, from, to).CountAsync();

        public Task<decimal> GetNewsPaperTotalPRAsync(int clientId, DateTime? from, DateTime? to)
            => Papers(clientId, from, to).SumAsync(n => n.PRValue ?? 0m);

        public Task<decimal> GetNewsPaperTotalADAsync(int clientId, DateTime? from, DateTime? to)
          => Papers(clientId, from, to).SumAsync(n => n.ADValue ?? 0m);


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


        public Task<int> GetVideoCountAsync(int clientId, DateTime? from, DateTime? to)
         => Videos(clientId, from, to).CountAsync();

        public Task<decimal> GetVideoTotalADAsync(int clientId, DateTime? from, DateTime? to)
            => Videos(clientId, from, to).SumAsync(v => v.ADValue ?? 0m);

        public Task<decimal> GetVideoTotalPRAsync(int clientId, DateTime? from, DateTime? to)
            => Videos(clientId, from, to).SumAsync(v => v.PRValue ?? 0m);


        // ── shared filter helpers ──────────────────────────────────────────────

        private IQueryable<ClientArticle> Articles(int clientId, DateTime? from, DateTime? to)
        {
            var q = _context.Set<ClientArticle>()
                            .Where(a => a.ClientId == clientId
                                     && a.IsActive
                                     && a.Deleted == 0);
            if (from.HasValue) q = q.Where(a => a.Date >= from.Value);
            if (to.HasValue) q = q.Where(a => a.Date <= to.Value.AddDays(1).AddTicks(-1));
            return q;
        }

        private IQueryable<ClientNewsPaper> Papers(int clientId, DateTime? from, DateTime? to)
        {
            var q = _context.Set<ClientNewsPaper>()
                            .Where(n => n.ClientId == clientId
                                     && n.IsActive
                                     && n.Deleted == 0);
            if (from.HasValue) q = q.Where(n => n.Date >= from.Value);
            if (to.HasValue) q = q.Where(n => n.Date <= to.Value.AddDays(1).AddTicks(-1));
            return q;
        }

        private IQueryable<ClientVideo> Videos(int clientId, DateTime? from, DateTime? to)
        {
            var q = _context.Set<ClientVideo>()
                            .Where(v => v.ClientId == clientId
                                     && v.IsActive
                                     && v.Deleted == 0);
            if (from.HasValue) q = q.Where(v => v.Date >= from.Value);
            if (to.HasValue) q = q.Where(v => v.Date <= to.Value.AddDays(1).AddTicks(-1));
            return q;
        }


    }
}
