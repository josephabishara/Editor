using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IReportNewspaperRepository : IRepository<ReportNewspaper>
    {
        Task<IEnumerable<ReportNewspaper>> GetByReportIdAsync(int reportId);
        Task DeleteByReportIdAsync(int reportId);
        Task AddRangeAsync(IEnumerable<ReportNewspaper> entities);
    }
}
