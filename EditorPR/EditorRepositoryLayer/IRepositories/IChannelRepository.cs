using EditorEntitiesLayer.Entities;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IChannelRepository : IRepository<Channel>
    {
        Task<IEnumerable<Channel>> GetActiveChannelsAsync();
        Task<IEnumerable<Channel>> SearchByNameAsync(string name);
    }
}
