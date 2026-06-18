using EditorDataLayer.Services;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.UserLogViewModel;

namespace EditorLogicLayer.UserLogLogic
{
    public class UserLogService : IUserLogService
    {
        private readonly IUserLogRepository _repo;
        private readonly ICurrentUserService _currentUser;

        public UserLogService(IUserLogRepository repo, ICurrentUserService currentUser)
        {
            _repo = repo;
            _currentUser = currentUser;
        }

        public async Task<(bool Success, string Message)> CreateAsync(UserLogDTO model)
        {
            var entity = new UserLog
            {
                // Who/when is always resolved server-side — never trust the caller.
                UserId = _currentUser.UserId,
                UserName = _currentUser.UserName,
                LogDate = DateTime.UtcNow,

                // What happened — supplied by the caller.
                Action = model.Action,
                ControllerName = string.IsNullOrWhiteSpace(model.ControllerName)
                    ? _currentUser.ControllerName
                    : model.ControllerName,
                EntityName = model.EntityName,
                RecordId = model.RecordId
            };

            await _repo.AddAsync(entity);
            return (true, "Log entry recorded.");
        }

        public async Task<IEnumerable<UserLogDTO>> GetRecentAsync(int take = 500)
            => (await _repo.GetRecentAsync(take)).Select(MapToDTO);

        public async Task<UserLogDTO?> GetByIdAsync(int id)
        {
            var log = await _repo.GetByIdAsync(id);
            return log == null ? null : MapToDTO(log);
        }

        public async Task<IEnumerable<UserLogDTO>> GetByRecordAsync(string entityName, int recordId)
            => (await _repo.GetByRecordAsync(entityName, recordId)).Select(MapToDTO);

        public async Task<IEnumerable<UserLogDTO>> GetByUserAsync(int userId)
            => (await _repo.GetByUserAsync(userId)).Select(MapToDTO);

        public async Task<IEnumerable<UserLogDTO>> GetByActionAsync(string action)
            => (await _repo.GetByActionAsync(action)).Select(MapToDTO);

        public async Task<IEnumerable<UserLogDTO>> GetByDateRangeAsync(DateTime from, DateTime to)
            => (await _repo.GetByDateRangeAsync(from, to)).Select(MapToDTO);

        private static UserLogDTO MapToDTO(UserLog l) => new()
        {
            LogId = l.LogId,
            UserId = l.UserId,
            UserName = l.UserName,
            LogDate = l.LogDate,
            Action = l.Action,
            ControllerName = l.ControllerName,
            EntityName = l.EntityName,
            RecordId = l.RecordId
        };
    }
}