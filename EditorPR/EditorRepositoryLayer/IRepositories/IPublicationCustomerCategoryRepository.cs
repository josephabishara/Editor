using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IPublicationCustomerCategoryRepository
    {
        Task<IEnumerable<PublicationCustomerCategory>> GetByClientIdAsync(int clientId);
        Task<PublicationCustomerCategory?> GetByIdAsync(int id);
        Task AddRangeAsync(IEnumerable<PublicationCustomerCategory> entities);
        Task UpdateAsync(PublicationCustomerCategory entity);
        Task UpdateRangeAsync(IEnumerable<PublicationCustomerCategory> entities);
        Task<bool> ExistsForClientAsync(int clientId);
    }
}
