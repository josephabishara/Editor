using EditorViewModelLayer.UserLogViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.UserLogLogic
{
    public interface IUserLogService
    {
        // Manual logging only — see usage note above. The automatic interceptor
        // in ApplicationDbContext covers all BaseEntity Create/Update/Delete already.
        Task<(bool Success, string Message)> CreateAsync(UserLogDTO model);

        Task<IEnumerable<UserLogDTO>> GetRecentAsync(int take = 500);
        Task<UserLogDTO?> GetByIdAsync(int id);
        Task<IEnumerable<UserLogDTO>> GetByRecordAsync(string entityName, int recordId);
        Task<IEnumerable<UserLogDTO>> GetByUserAsync(int userId);
        Task<IEnumerable<UserLogDTO>> GetByActionAsync(string action);
        Task<IEnumerable<UserLogDTO>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}
