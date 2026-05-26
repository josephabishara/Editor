using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IGeneralArticleRepository : IRepository<GeneralArticle>
    {
        Task<IEnumerable<GeneralArticle>> GetActiveAsync();
    }
}
