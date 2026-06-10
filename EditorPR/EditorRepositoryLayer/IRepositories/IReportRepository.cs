using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IReportRepository : IRepository<Report>
    {
        Task<IEnumerable<Report>> GetActiveAsync();
        Task<Report?> GetByIdWithNavAsync(int id);   // includes Customer, Articles, Newspapers
        Task<IEnumerable<Report>> GetByClientIdAsync(int clientId);
    }
}
