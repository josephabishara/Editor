using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{

    public interface IClientVideoRepository : IRepository<ClientVideo>
    {
        Task<IEnumerable<ClientVideo>> GetByClientIdAsync(int clientId);
        Task<ClientVideo?> GetByIdWithDetailsAsync(int id);
    }
}