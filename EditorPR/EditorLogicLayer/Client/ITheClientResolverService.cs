using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Client
{
    public interface ITheClientResolverService
    {
        Task<int?> ResolveClientIdAsync(int applicationUserId);
    }
}
