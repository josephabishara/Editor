using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface INewsRepository : IRepository<News>
    {
        Task<IEnumerable<News>> GetActiveNewsAsync();
        Task<IEnumerable<News>> GetBySourceTypeAsync(string sourceType);
    }
}
