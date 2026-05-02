using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IWebsiteRepository : IRepository<Websites>
    {
        Task<IEnumerable<Websites>> GetActiveWebsitesAsync();
        Task<IEnumerable<Websites>> SearchByNameAsync(string name);
    }
}
