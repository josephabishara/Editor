using EditorLogicLayer.ClientNewsPaperLogic;
using EditorLogicLayer.GeneralNewspaper;
using EditorViewModelLayer.GeneralNewspaperViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    public class GeneralNewspaperController : Controller
    {
        private readonly IGeneralNewspaperService _service;
        private readonly IClientNewsPaperService _clientNewsPaperService;


        public GeneralNewspaperController(
            IGeneralNewspaperService service,
            IClientNewsPaperService clientNewsPaperService)
        {
            _service = service;
            _clientNewsPaperService = clientNewsPaperService;
        }

        // ── Index ──────────────────────────────────────────────────────────────
        // Filters: From/To Date, Title (contains), Publication (select)

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Index(GeneralNewspaperFilterDTO filter)
        {
            var vm = await _service.GetIndexViewModelAsync(filter);
            return View(vm);
        }

        // ── Details ────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var newspaper = await _service.GetByIdAsync(id);
            if (newspaper == null) return NotFound();
            return View(newspaper);
        }

        // ── Create ─────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            var model = new GeneralNewspaperDTO { Date = DateTime.Today };
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GeneralNewspaperDTO model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var (success, message) = await _service.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync(model);
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
            var newspaper = await _service.GetByIdAsync(id);
            if (newspaper == null) return NotFound();
            await PopulateDropdownsAsync(newspaper);
            return View(newspaper);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GeneralNewspaperDTO model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var (success, message) = await _service.UpdateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync(model);
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
            var newspaper = await _service.GetByIdAsync(id);
            if (newspaper == null) return NotFound();
            return View(newspaper);
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

        // POST: /GeneralNewspaper/ExportSelected
        // Receives a comma-separated list of selected IDs from the Index checkbox form
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportSelected(string selectedIds)
        {
            if (string.IsNullOrWhiteSpace(selectedIds))
            {
                TempData["Error"] = "No newspapers selected for export.";
                return RedirectToAction(nameof(Index));
            }

            var ids = selectedIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                .Where(n => n > 0)
                .ToList();

            if (!ids.Any())
            {
                TempData["Error"] = "No valid newspapers selected.";
                return RedirectToAction(nameof(Index));
            }

            // Fetch only selected rows then map to DTO for export
            var all = await _service.GetAllAsync();
            var selected = all.Where(a => ids.Contains(a.Id));

            var fileBytes = _service.ExportToExcel(selected);
            var fileName = $"GeneralNewspapers_Selected_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ── Export All / Export Filtered ──────────────────────────────────────
        // Same filter parameters as Index, so the downloaded file always matches
        // what's currently shown on screen.

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> ExportToExcel([FromQuery] GeneralNewspaperFilterDTO filter)
        {
            var newspapers = await _service.GetFilteredAsync(filter);
            var fileBytes = _service.ExportToExcel(newspapers);
            var fileName = $"GeneralNewspapers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

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
            var bytes = _service.ExportToExcel(Enumerable.Empty<GeneralNewspaperDTO>());
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "GeneralNewspapers_ImportTemplate.xlsx");
        }

        // ── Share to Clients ───────────────────────────────────────────────────
        // Creates one ClientNewsPaper per selected client from this GeneralNewspaper.
        // No new master row is created — every ClientNewsPaper.NewsPaperId points
        // back at this same row. Category/SubCategory are chosen per-client
        // (ClientCategories is scoped by ClientId), so the modal loads those via
        // AJAX once a client is checked.

        // GET: /GeneralNewspaper/GetShareClients/5
        // Returns the full client checklist for the Share modal.
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetShareClients(int id)
        {
            var options = await _clientNewsPaperService.GetShareClientOptionsAsync(id);
            return Json(options.Select(o => new
            {
                clientId = o.ClientId,
                clientName = o.ClientName,
                alreadyShared = o.AlreadyShared
            }));
        }

        // GET: /GeneralNewspaper/GetShareCategories?clientId=5
        // Top-level categories for one client row in the modal.
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetShareCategories(int clientId)
        {
            var opts = await _clientNewsPaperService.GetCategoryOptionsAsync(clientId);
            return Json(opts.Select(o => new { value = o.Value, text = o.Text }));
        }

        // GET: /GeneralNewspaper/GetShareSubCategories?parentId=12
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetShareSubCategories(int parentId)
        {
            var opts = await _clientNewsPaperService.GetSubCategoryOptionsAsync(parentId);
            return Json(opts.Select(o => new { value = o.Value, text = o.Text }));
        }

        // POST: /GeneralNewspaper/ShareToClients
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShareToClients(ShareNewspaperToClientsDTO model)
        {
            var (success, message, count) = await _clientNewsPaperService.ShareToClientsAsync(model);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id = model.GeneralNewspaperId });
        }
        // ── Helper ─────────────────────────────────────────────────────────────

        private async Task PopulateDropdownsAsync(GeneralNewspaperDTO model)
        {
            model.PublicationOptions = await _service.GetPublicationOptionsAsync(model.PublicationId);
            model.WriterOptions = await _service.GetWriterOptionsAsync(model.WriterId);
            model.BrandingOptions = EditorViewModelLayer.MediaViewModel.MediaOptions.BrandingList();
            model.ToningOptions = EditorViewModelLayer.MediaViewModel.MediaOptions.ToningList();
            model.YesNoOptions = EditorViewModelLayer.MediaViewModel.MediaOptions.YesNoList();
        }
    }
}
