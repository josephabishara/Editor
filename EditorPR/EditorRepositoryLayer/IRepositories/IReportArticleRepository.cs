using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IReportArticleRepository : IRepository<ReportArticle>
    {
        Task<IEnumerable<ReportArticle>> GetByReportIdAsync(int reportId);
        Task DeleteByReportIdAsync(int reportId);
        Task AddRangeAsync(IEnumerable<ReportArticle> entities);
    }
}
