using EditorEntitiesLayer.Entities;
using EditorViewModelLayer.AuthViewModels;
using Microsoft.AspNetCore.Identity;

namespace EditorLogicLayer.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager; 
       
        public static readonly string[] Roles =
        {
            "Admin", "Manager", "EditorWeb", "Auditor", "Client", "Assistant"
        };

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // ── Auth ──────────────────────────────────────────────────────────────
        public async Task SeedRolesAsync()
        {
            foreach (var role in Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new ApplicationRole { Name = role });  
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return (false, "Email is already registered.");

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Username,
                Email = model.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            await _userManager.AddToRoleAsync(user, model.Role ?? "Client");
            return (true, "User registered successfully.");
        }

        public async Task<(bool Success, string Message)> LoginAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return (false, "Invalid email or password.");

            if (!user.IsActive)
                return (false, "Your account has been deactivated.");

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
                return (true, "Login successful.");

            if (result.IsLockedOut)
                return (false, "Account locked due to multiple failed attempts. Try again later.");

            return (false, "Invalid email or password.");
        }

        public async Task LogoutAsync()
            => await _signInManager.SignOutAsync();

       
        // ── User Management ───────────────────────────────────────────────────

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserDTO>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserDTO
                {
                    Id = u.Id,
                    FullName = u.FullName ?? string.Empty,
                    Username = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "—",
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                });
            }

            return result.OrderBy(u => u.FullName);
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "—",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(RegisterViewModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return (false, "Email is already registered.");

            if (await _userManager.FindByNameAsync(model.Username) != null)
                return (false, "Username is already taken.");

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Username,
                Email = model.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            await _userManager.AddToRoleAsync(user, model.Role ?? "Client");
            return (true, $"User '{user.UserName}' created successfully.");
        }

        public async Task<(bool Success, string Message)> EditUserAsync(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
                return (false, "User not found.");

            // Check email uniqueness against other users
            var emailOwner = await _userManager.FindByEmailAsync(model.Email);
            if (emailOwner != null && emailOwner.Id != model.Id)
                return (false, "Email is already used by another user.");

            // Check username uniqueness against other users
            var nameOwner = await _userManager.FindByNameAsync(model.Username);
            if (nameOwner != null && nameOwner.Id != model.Id)
                return (false, "Username is already taken by another user.");

            user.FullName = model.FullName;
            user.UserName = model.Username;
            user.Email = model.Email;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return (false, errors);
            }

            // Re-assign role if changed
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return (true, "User updated successfully.");
        }

        public async Task<(bool Success, string Message)> ToggleActiveAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (false, "User not found.");

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var status = user.IsActive ? "activated" : "deactivated";
            return (true, $"User '{user.UserName}' has been {status}.");
        }

        // ── Password ──────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> ChangeMyPasswordAsync(int userId, ChangeMyPasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (false, "User not found.");

            var result = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            // Refresh sign-in cookie so the user isn't logged out
            await _signInManager.RefreshSignInAsync(user);
            return (true, "Password changed successfully.");
        }

        public async Task<(bool Success, string Message)> AdminChangePasswordAsync(AdminChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
                return (false, "User not found.");

            // Admin reset: no need for current password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            return (true, $"Password for '{user.UserName}' reset successfully.");
        }

        // ── Roles ─────────────────────────────────────────────────────────────

        public string[] GetAllRoles() => Roles;

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }
    }
}

