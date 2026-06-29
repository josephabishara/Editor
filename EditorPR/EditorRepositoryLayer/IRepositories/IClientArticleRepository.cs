using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IClientArticleRepository : IRepository<ClientArticle>
    {
        Task<IEnumerable<ClientArticle>> GetByClientIdAsync(int clientId);
        Task<ClientArticle?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<ClientArticle>> GetChildrenAsync(int parentId);
        Task<IEnumerable<ClientArticle>> GetByClientIdAsync(int clientId, DateTime? from, DateTime? to);
        Task<IEnumerable<int>> GetClientIdsByArticleIdAsync(int generalArticleId);


    }
}
