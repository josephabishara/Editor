using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IClientNewsPaperRepository : IRepository<ClientNewsPaper>
    {
        Task<IEnumerable<ClientNewsPaper>> GetByClientIdAsync(int clientId);
        Task<ClientNewsPaper?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<ClientNewsPaper>> GetByNewsPaperIdAsync(int newsPaperId);

        Task<IEnumerable<ClientNewsPaper>> GetChildrenAsync(int parentId);

        Task<IEnumerable<ClientNewsPaper>> GetByClientIdAsync(int clientId, DateTime? from, DateTime? to);


    }
}
