using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IClientCategoryRepository : IRepository<ClientCategories>
    {
        Task<IEnumerable<ClientCategories>> GetByClientAsync(int clientId);
        Task<IEnumerable<ClientCategories>> GetRootCategoriesByClientAsync(int clientId);
        Task<IEnumerable<ClientCategories>> GetChildrenAsync(int parentId);
        Task<bool> CategoryNameExistsAsync(int clientId, string name, int? excludeId = null);

    }
}
