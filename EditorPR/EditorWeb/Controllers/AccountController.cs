using EditorLogicLayer.Auth;
using EditorViewModelLayer.AuthViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EditorEntitiesLayer.Entities;

namespace EditorWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] _staffRoles =
            { "Admin", "Manager", "EditorWeb", "Auditor" };

        private static readonly string[] _clientRoles =
            { "Client", "Assistant" };

        public AccountController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        // ── GET /Account/Login  (shared view, tab = Editor) ───────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectBasedOnRole();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ── POST /Account/Login  (Admin | Manager | EditorWeb | Auditor) ──────
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _authService.LoginAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            // Role guard — only staff roles may use this endpoint
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                bool isStaff = roles.Any(r => _staffRoles.Contains(r));

                if (!isStaff)
                {
                    // Wrong portal — sign them out and show error
                    await _authService.LogoutAsync();
                    ModelState.AddModelError(string.Empty,
                        "This portal is for staff only. Please use the Client login.");
                    return View(model);
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        // ── POST /Account/ClientLogin  (Client | Assistant) ───────────────────
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClientLogin(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var (success, message) = await _authService.LoginAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View("Login", model);
            }

            // Role guard — only client roles may use this endpoint
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                bool isClient = roles.Any(r => _clientRoles.Contains(r));

                if (!isClient)
                {
                    await _authService.LogoutAsync();
                    ModelState.AddModelError(string.Empty,
                        "This portal is for clients only. Please use the Editor login.");
                    return View("Login", model);
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "MyDashboard");
        }

        // ── GET /Account/Register  (Admin only) ───────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register() => View();

        // ── POST /Account/Register ────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _authService.RegisterAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Login));
        }

        // ── POST /Account/Logout ──────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction(nameof(Login));
        }

        // ── GET /Account/AccessDenied ─────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Redirect an already-authenticated user to the correct portal
        /// without needing a separate User.IsInRole() call in the view.
        /// </summary>
        private IActionResult RedirectBasedOnRole()
        {
            if (User.IsInRole("Client") || User.IsInRole("Assistant"))
                return RedirectToAction("Index", "MyDashboard");

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
