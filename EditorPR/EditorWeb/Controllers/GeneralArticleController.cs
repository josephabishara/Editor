using EditorLogicLayer.ClientArticleLogic;
using EditorLogicLayer.GeneralArticle;
using EditorViewModelLayer.GeneralArticleViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    public class GeneralArticleController : Controller
    {
        private readonly IGeneralArticleService _service;
        private readonly IClientArticleService _clientArticleService;

        public GeneralArticleController(IGeneralArticleService service, IClientArticleService clientArticleService)
        {
            _service = service;
            _clientArticleService = clientArticleService;
        }
            

        // ── Index ──────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Index(GeneralArticleFilterDTO filter)
        {
            var vm = await _service.GetIndexViewModelAsync(filter);
            return View(vm);
        }

        // ── Details ────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var article = await _service.GetByIdAsync(id);
            if (article == null) return NotFound();
            return View(article);
        }

        // ── Create ─────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create() => View(new GeneralArticleDTO { Date = DateTime.Today });

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GeneralArticleDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _service.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Edit ───────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _service.GetByIdAsync(id);
            if (article == null) return NotFound();
            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GeneralArticleDTO model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _service.UpdateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Delete ─────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _service.GetByIdAsync(id);
            if (article == null) return NotFound();
            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _service.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Export Selected ────────────────────────────────────────────────────

        // POST: /GeneralArticle/ExportSelected
        // Receives a comma-separated list of selected IDs from the Index checkbox form
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportSelected(string selectedIds)
        {
            if (string.IsNullOrWhiteSpace(selectedIds))
            {
                TempData["Error"] = "No articles selected for export.";
                return RedirectToAction(nameof(Index));
            }

            var ids = selectedIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                .Where(n => n > 0)
                .ToList();

            if (!ids.Any())
            {
                TempData["Error"] = "No valid articles selected.";
                return RedirectToAction(nameof(Index));
            }

            // Fetch only selected rows then map to DTO for export
            var all = await _service.GetAllAsync();
            var selected = all.Where(a => ids.Contains(a.Id));

            var fileBytes = _service.ExportToExcel(selected);
            var fileName = $"GeneralArticles_Selected_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ── Export All ─────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> ExportToExcel()
        {
            var articles = await _service.GetAllAsync();
            var fileBytes = _service.ExportToExcel(articles);
            var fileName = $"GeneralArticles_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ── Import ─────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Import() => View();

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select an Excel file.");
                return View();
            }

            using var stream = file.OpenReadStream();
            var (success, message, count) =
                await _service.ImportFromExcelAsync(stream, file.FileName);

            if (!success)
            {
                TempData["Error"] = message;
                return View();
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Download Template ──────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DownloadTemplate()
        {
            var bytes = _service.ExportToExcel(Enumerable.Empty<GeneralArticleDTO>());
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "GeneralArticles_ImportTemplate.xlsx");
        }


        // ── Share to Clients ───────────────────────────────────────────────────
        // Creates one ClientArticle per selected client from this GeneralArticle.
        // No new GeneralArticle is created — every ClientArticle.ArticleId points
        // back at this same row. Category/SubCategory are chosen per-client
        // (ClientCategories is scoped by ClientId), so the modal loads those via
        // AJAX once a client is checked.

        // GET: /GeneralArticle/GetShareClients/5
        // Returns the full client checklist for the Share modal.
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetShareClients(int id)
        {
            var options = await _clientArticleService.GetShareClientOptionsAsync(id);
            return Json(options.Select(o => new
            {
                clientId = o.ClientId,
                clientName = o.ClientName,
                alreadyShared = o.AlreadyShared
            }));
        }

        // GET: /GeneralArticle/GetShareCategories?clientId=5
        // Top-level categories for one client row in the modal.
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetShareCategories(int clientId)
        {
            var opts = await _clientArticleService.GetCategoryOptionsAsync(clientId);
            return Json(opts.Select(o => new { value = o.Value, text = o.Text }));
        }

        // GET: /GeneralArticle/GetShareSubCategories?parentId=12
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetShareSubCategories(int parentId)
        {
            var opts = await _clientArticleService.GetSubCategoryOptionsAsync(parentId);
            return Json(opts.Select(o => new { value = o.Value, text = o.Text }));
        }

        // POST: /GeneralArticle/ShareToClients
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShareToClients(ShareArticleToClientsDTO model)
        {
            var (success, message, count) = await _clientArticleService.ShareToClientsAsync(model);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id = model.GeneralArticleId });
        }



        // Helper method:
        //private async Task PopulateDropdownsAsync()
        //{
        //    var websites = await _websiteService.GetAllAsync();
        //    var writers = await _writerService.GetAllAsync();

        //    ViewBag.Websites = websites.Select(w => new SelectListItem
        //    {
        //        Value = w.Id.ToString(),
        //        Text = w.WebsiteName
        //    });

        //    ViewBag.Writers = writers.Select(w => new SelectListItem
        //    {
        //        Value = w.Id.ToString(),
        //        Text = w.WriterName
        //    });
        //}

    }
}