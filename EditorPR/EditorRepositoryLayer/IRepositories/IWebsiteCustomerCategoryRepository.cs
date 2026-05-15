using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IWebsiteCustomerCategoryRepository
    {
        Task<IEnumerable<WebsiteCustomerCategory>> GetByClientIdAsync(int clientId);
        Task<WebsiteCustomerCategory?> GetByIdAsync(int id);
        Task AddRangeAsync(IEnumerable<WebsiteCustomerCategory> entities);
        Task UpdateAsync(WebsiteCustomerCategory entity);
        Task UpdateRangeAsync(IEnumerable<WebsiteCustomerCategory> entities);
        Task<bool> ExistsForClientAsync(int clientId);
    }
}
