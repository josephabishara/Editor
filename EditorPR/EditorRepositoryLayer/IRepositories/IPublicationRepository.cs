using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IPublicationRepository : IRepository<Publication>
    {
        Task<IEnumerable<Publication>> GetActivePublicationsAsync();
        Task<IEnumerable<Publication>> SearchByNameAsync(string name);
    }
}
