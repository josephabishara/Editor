using EditorLogicLayer.Auth;
using EditorViewModelLayer.AuthViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EditorWeb.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
            => _authService = authService;

        // ── User List ─────────────────────────────────────────────────────────

        // GET: /User
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index()
        {
            var users = await _authService.GetAllUsersAsync();
            return View(users);
        }

        // ── Add User ──────────────────────────────────────────────────────────

        // GET: /User/Create
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Roles = _authService.GetAllRoles();
            return View(new RegisterViewModel());
        }

        // POST: /User/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _authService.GetAllRoles();
                return View(model);
            }

            var (success, message) = await _authService.CreateUserAsync(model);
            if (!success)
            {
                ViewBag.Roles = _authService.GetAllRoles();
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Edit User ─────────────────────────────────────────────────────────

        // GET: /User/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = _authService.GetAllRoles();
            return View(new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            });
        }

        // POST: /User/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _authService.GetAllRoles();
                return View(model);
            }

            var (success, message) = await _authService.EditUserAsync(model);
            if (!success)
            {
                ViewBag.Roles = _authService.GetAllRoles();
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Toggle Active / Deactivate ────────────────────────────────────────

        // POST: /User/ToggleActive/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            // Prevent admin from deactivating themselves
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Index));
            }

            var (success, message) = await _authService.ToggleActiveAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Change MY Password ────────────────────────────────────────────────

        // GET: /User/ChangeMyPassword
        [HttpGet]
        public IActionResult ChangeMyPassword() => View(new ChangeMyPasswordViewModel());

        // POST: /User/ChangeMyPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeMyPassword(ChangeMyPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (success, message) = await _authService.ChangeMyPasswordAsync(userId, model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), "Home");
        }

        // ── Admin: Change Another User's Password ─────────────────────────────

        // GET: /User/AdminChangePassword/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminChangePassword(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return View(new AdminChangePasswordViewModel
            {
                UserId = user.Id,
                UserName = user.Username
            });
        }

        // POST: /User/AdminChangePassword
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminChangePassword(AdminChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _authService.AdminChangePasswordAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}
