using EditorDataLayer.Data;
using EditorRepositoryLayer.IRepositories;
using EditorEntitiesLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace EditorRepositoryLayer.Repositories
{
    public class ChannelRepository : GenericRepository<Channel>, IChannelRepository
    {
        public ChannelRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Channel>> GetActiveChannelsAsync()
            => await _dbSet
                .Where(c => c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.ChannelName)
                .ToListAsync();

        public async Task<IEnumerable<Channel>> SearchByNameAsync(string name)
            => await _dbSet
                .Where(c => c.ChannelName.Contains(name) && c.IsActive && c.Deleted == 0)
                .ToListAsync();
    }
}
