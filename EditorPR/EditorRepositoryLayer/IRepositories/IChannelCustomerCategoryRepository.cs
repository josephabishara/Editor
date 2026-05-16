using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IChannelCustomerCategoryRepository
    {
        Task<IEnumerable<ChannelCustomerCategory>> GetByClientIdAsync(int clientId);
        Task<ChannelCustomerCategory?> GetByIdAsync(int id);
        Task AddRangeAsync(IEnumerable<ChannelCustomerCategory> entities);
        Task UpdateAsync(ChannelCustomerCategory entity);
        Task UpdateRangeAsync(IEnumerable<ChannelCustomerCategory> entities);
        Task<bool> ExistsForClientAsync(int clientId);
    }
}
