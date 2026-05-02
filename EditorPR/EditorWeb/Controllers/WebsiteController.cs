using EditorLogicLayer;
using EditorLogicLayer.Website;
using EditorViewModelLayer;
using EditorViewModelLayer.WebsiteViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    //[AllowAnonymous]
    public class WebsiteController : Controller
    {
        private readonly IWebsiteService _websiteService;

        public WebsiteController(IWebsiteService websiteService)
            => _websiteService = websiteService;

        // GET: /Website
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        //[AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var websites = await _websiteService.GetAllAsync();
            return View(websites);
        }

        // GET: /Website/Details/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        //[AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var website = await _websiteService.GetByIdAsync(id);
            if (website == null) return NotFound();
            return View(website);
        }

        // GET: /Website/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        //[AllowAnonymous]
        public IActionResult Create() => View(new WebsiteDTO());

        // POST: /Website/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebsiteDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _websiteService.CreateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Website/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var website = await _websiteService.GetByIdAsync(id);
            if (website == null) return NotFound();
            return View(website);
        }

        // POST: /Website/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WebsiteDTO model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _websiteService.UpdateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Website/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var website = await _websiteService.GetByIdAsync(id);
            if (website == null) return NotFound();
            return View(website);
        }

        // POST: /Website/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _websiteService.DeleteAsync(id);

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Export to Excel ────────────────────────────────────────────────────

        // GET: /Website/ExportToExcel
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportToExcel()
        {
            var websites = await _websiteService.GetAllAsync();
            var fileBytes = _websiteService.ExportToExcel(websites);

            var fileName = $"Websites_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ── Import from Excel ──────────────────────────────────────────────────

        // GET: /Website/Import  — shows the upload form
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        //[AllowAnonymous]
        public IActionResult Import() => View();

        // POST: /Website/Import
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var (success, message, count) = await _websiteService.ImportFromExcelAsync(file);

            if (!success)
            {
                TempData["Error"] = message;
                return View();
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Website/DownloadTemplate
        // Downloads a blank Excel template showing the correct column structure
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        //[AllowAnonymous]
        public IActionResult DownloadTemplate()
        {
            var template = _websiteService.ExportToExcel(new List<WebsiteDTO>());
            return File(
                template,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Websites_ImportTemplate.xlsx");
        }
    }
}
