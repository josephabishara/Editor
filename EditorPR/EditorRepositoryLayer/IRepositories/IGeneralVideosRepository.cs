using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IGeneralVideosRepository : IRepository<GeneralVideos>
    {
        Task<IEnumerable<GeneralVideos>> GetActiveAsync();
    }
}
