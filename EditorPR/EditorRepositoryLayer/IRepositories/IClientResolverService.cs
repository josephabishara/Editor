using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IClientResolverService
    {
        Task<int?> ResolveClientIdAsync(int applicationUserId);
    }
}
