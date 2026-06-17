using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.Repositories
{
    public class ClientResolverService : IClientResolverService
    {
        private readonly ApplicationDbContext _context;

        public ClientResolverService(ApplicationDbContext context)
            => _context = context;

        /// <inheritdoc />
        public async Task<int?> ResolveClientIdAsync(int applicationUserId)
        {
            // 1. Try Client table first
            var clientId = await _context.Set<EditorEntitiesLayer.Entities.Client>()
                .Where(c => c.ApplicationUserId == applicationUserId && c.Deleted == 0)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();

            if (clientId.HasValue)
                return clientId;

            // 2. Fall back to Assistant → get parent ClientId
            var assistantClientId = await _context.Set<Assistant>()
                .Where(a => a.ApplicationUserId == applicationUserId && a.Deleted == 0)
                .Select(a => (int?)a.ClientId)
                .FirstOrDefaultAsync();

            return assistantClientId;
        }
    }
}
