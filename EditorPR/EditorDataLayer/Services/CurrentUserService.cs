using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace EditorDataLayer.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private HttpContext? Context => _accessor.HttpContext;

        public bool IsAuthenticated => Context?.User?.Identity?.IsAuthenticated ?? false;

        public int UserId
        {
            get
            {
                var raw = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(raw, out var id) ? id : 0;
            }
        }

        public string UserName =>
            IsAuthenticated ? (Context!.User.Identity!.Name ?? "Unknown") : "System";

        public string ControllerName =>
            Context?.Request.RouteValues.TryGetValue("controller", out var c) == true
                ? c?.ToString() ?? "System"
                : "System";

        public string ActionName =>
            Context?.Request.RouteValues.TryGetValue("action", out var a) == true
                ? a?.ToString() ?? string.Empty
                : string.Empty;
    }
}
