using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IGeneralArticleRepository : IRepository<GeneralArticle>
    {
        Task<IEnumerable<GeneralArticle>> GetActiveAsync();
        Task<GeneralArticle?> GetByIdWithNavAsync(int id);   // includes Website + Writer
        Task<IEnumerable<GeneralArticle>> GetByIdsAsync(IEnumerable<int> ids);
    }
}
