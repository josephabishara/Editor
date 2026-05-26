using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface INewsPaperRepository : IRepository<NewsPaper>
    {
        Task<IEnumerable<NewsPaper>> GetActiveAsync();
    }
}
