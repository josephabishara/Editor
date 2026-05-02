using EditorViewModelLayer.AuthViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);

        // ── Auth ──────────────────────────────────────────────────────────────
        Task<(bool Success, string Message)> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task SeedRolesAsync();

        // ── User Management ───────────────────────────────────────────────────
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(int id);
        Task<(bool Success, string Message)> CreateUserAsync(RegisterViewModel model);
        Task<(bool Success, string Message)> EditUserAsync(EditUserViewModel model);
        Task<(bool Success, string Message)> ToggleActiveAsync(int userId);

        // ── Password ──────────────────────────────────────────────────────────
        Task<(bool Success, string Message)> ChangeMyPasswordAsync(int userId, ChangeMyPasswordViewModel model);
        Task<(bool Success, string Message)> AdminChangePasswordAsync(AdminChangePasswordViewModel model);

        // ── Roles ─────────────────────────────────────────────────────────────
        Task<bool> AssignRoleAsync(string userId, string role);
        string[] GetAllRoles();
    }
}
