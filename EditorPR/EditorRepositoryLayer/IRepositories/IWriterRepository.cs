using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IWriterRepository : IRepository<Writer>
    {
        Task<IEnumerable<Writer>> GetActiveWritersAsync();
        Task<IEnumerable<Writer>> SearchByNameAsync(string name);
    }
}
