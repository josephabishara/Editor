using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IClientNewsRepository : IRepository<ClientNews>
    {
        /// <summary>All active ClientNews for a client, News navigation included.</summary>
        Task<IEnumerable<ClientNews>> GetByClientIdAsync(int clientId);

        /// <summary>Single row with News + Client + Writer navigation.</summary>
        Task<ClientNews?> GetByIdWithDetailsAsync(int id);

        /// <summary>Filter by SourceType.</summary>
        Task<IEnumerable<ClientNews>> GetByClientAndSourceTypeAsync(int clientId, string sourceType);

        /// <summary>All clients that already have a given News master.</summary>
        Task<IEnumerable<ClientNews>> GetByNewsIdAsync(int newsId);
    }
}
