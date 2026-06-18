using System;
using System.Collections.Generic;
using System.Text;

namespace EditorDataLayer.Services
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string UserName { get; }
        bool IsAuthenticated { get; }
        string ControllerName { get; }
        string ActionName { get; }
    }
}
