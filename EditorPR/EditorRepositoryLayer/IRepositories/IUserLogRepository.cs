using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IUserLogRepository
    {
        Task AddAsync(UserLog log);
        Task<UserLog?> GetByIdAsync(int id);
        Task<IEnumerable<UserLog>> GetRecentAsync(int take = 500);
        Task<IEnumerable<UserLog>> GetByRecordAsync(string entityName, int recordId);
        Task<IEnumerable<UserLog>> GetByUserAsync(int userId);
        Task<IEnumerable<UserLog>> GetByActionAsync(string action);
        Task<IEnumerable<UserLog>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}
