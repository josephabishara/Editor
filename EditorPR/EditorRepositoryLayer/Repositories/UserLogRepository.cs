using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class UserLogRepository : GenericRepository<UserLog>, IUserLogRepository
    {
        public UserLogRepository(ApplicationDbContext context) : base(context) { }

        // AddAsync(UserLog) and GetByIdAsync(int) are satisfied by GenericRepository<UserLog>.
        // ToListAsync
        public async Task<IEnumerable<UserLog>> GetRecentAsync(int take = 500)
            => await _dbSet
                .OrderByDescending(l => l.LogDate)
                .Take(take)
                .ToListAsync();

        public async Task<IEnumerable<UserLog>> GetByRecordAsync(string entityName, int recordId)
            => await _dbSet
                .Where(l => l.EntityName == entityName && l.RecordId == recordId)
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();

        public async Task<IEnumerable<UserLog>> GetByUserAsync(int userId)
            => await _dbSet
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();

        public async Task<IEnumerable<UserLog>> GetByActionAsync(string action)
            => await _dbSet
                .Where(l => l.Action == action)
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();

        public async Task<IEnumerable<UserLog>> GetByDateRangeAsync(DateTime from, DateTime to)
            => await _dbSet
                .Where(l => l.LogDate >= from && l.LogDate <= to)
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();
    }
}
