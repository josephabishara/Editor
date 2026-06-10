using EditorLogicLayer.Reports;
using EditorViewModelLayer.ReportViewModel;
using EditorEntitiesLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    public class ReportController : Controller
    {
        private readonly IReportService _service;

        public ReportController(IReportService service) => _service = service;

        // ── Index ────────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Index()
        {
            var reports = await _service.GetAllAsync();
            return View(reports);
        }

        // ── Details ──────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var report = await _service.GetDetailsAsync(id);
            if (report == null) return NotFound();
            return View(report);
        }

        // ── Preview (standalone HTML page, no layout) ─────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Preview(int id)
        {
            var report = await _service.GetDetailsAsync(id);
            if (report == null) return NotFound();
            return View(report);   // uses Views/Report/Preview.cshtml with layout = null
        }

        // ── WIZARD STEP 1: Create ─────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new ReportDTO { ReportDate = DateTime.Today });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportDTO model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            var (success, message, reportId) = await _service.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(SelectArticles), new { id = reportId });
        }

        // ── WIZARD STEP 2: Select Articles ────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SelectArticles(int id, DateTime? from, DateTime? to)
        {
            var report = await _service.GetByIdAsync(id);
            if (report == null) return NotFound();

            var articles = await _service.GetArticlePickerAsync(id, from, to);

            var vm = new SaveArticlesViewModel
            {
                ReportId = id,
                From = from,
                To = to,
                SelectedArticleIds = articles.Where(a => a.Selected).Select(a => a.ArticleId).ToList()
            };

            ViewBag.Report = report;
            ViewBag.Articles = articles;
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectArticles(SaveArticlesViewModel model)
        {
            var (success, message) = await _service.SaveArticlesAsync(
                model.ReportId, model.SelectedArticleIds);

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(SelectArticles), new { id = model.ReportId });
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(SelectNewspapers), new { id = model.ReportId });
        }

        // ── WIZARD STEP 3: Select Newspapers ──────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SelectNewspapers(int id, DateTime? from, DateTime? to)
        {
            var report = await _service.GetByIdAsync(id);
            if (report == null) return NotFound();

            var newspapers = await _service.GetNewspaperPickerAsync(id, from, to);

            var vm = new SaveNewspapersViewModel
            {
                ReportId = id,
                From = from,
                To = to,
                SelectedNewspaperIds = newspapers.Where(n => n.Selected).Select(n => n.NewspaperId).ToList()
            };

            ViewBag.Report = report;
            ViewBag.Newspapers = newspapers;
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectNewspapers(SaveNewspapersViewModel model)
        {
            var (success, message) = await _service.SaveNewspapersAsync(
                model.ReportId, model.SelectedNewspaperIds);

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(SelectNewspapers), new { id = model.ReportId });
            }

            TempData["Success"] = "Report completed successfully.";
            return RedirectToAction(nameof(Details), new { id = model.ReportId });
        }

        // ── Edit ──────────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var report = await _service.GetByIdAsync(id);
            if (report == null) return NotFound();
            await PopulateDropdownsAsync();
            return View(report);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReportDTO model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            var (success, message) = await _service.UpdateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Delete ────────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _service.GetByIdAsync(id);
            if (report == null) return NotFound();
            return View(report);
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

        // ── Publish ───────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var (success, message) = await _service.PublishAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── UnPublish ─────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnPublish(int id)
        {
            var (success, message) = await _service.UnPublishAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private async Task PopulateDropdownsAsync()
        {
            var clients = await _service.GetClientOptionsAsync();
            ViewBag.Customers = clients.Select(c => new SelectListItem
            {
                Value = c.Value,
                Text = c.Text
            });

            ViewBag.ReportTypes = Enum.GetValues<ReportType>()
                .Select(rt => new SelectListItem
                {
                    Value = ((int)rt).ToString(),
                    Text = rt.ToString()
                });
        }
    }
}
