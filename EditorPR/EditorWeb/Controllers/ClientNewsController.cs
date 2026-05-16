using EditorLogicLayer.News;
using EditorViewModelLayer.NewsViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]

    public class ClientNewsController : Controller
    {
        private readonly IClientNewsService _newsService;

        public ClientNewsController(IClientNewsService newsService)
            => _newsService = newsService;

        // ══════════════════════════════════════════════════════════════════════
        // INDEX
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Index(
            int clientId,
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int writerId = 0,
            int publicationId = 0,
            string? sourceType = null,
            bool? published = null)
        {
            var dashboard = await _newsService.GetClientNewsDashboardAsync(clientId);
            var items = dashboard.Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
                items = items.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(dateFrom, out var from))
                items = items.Where(i => i.Date.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to))
                items = items.Where(i => i.Date.Date <= to.Date);
            if (categoryId > 0) items = items.Where(i => i.CategoryId == categoryId);
            if (subCategoryId > 0) items = items.Where(i => i.SubCategoryId == subCategoryId);
            if (writerId > 0) items = items.Where(i => i.WriterId == writerId);
            if (publicationId > 0) items = items.Where(i => i.publicationId == publicationId);
            if (!string.IsNullOrWhiteSpace(sourceType))
                items = items.Where(i => i.SourceType == sourceType);
            if (published.HasValue)
                items = items.Where(i => i.Publish == published.Value);

            // Build filter SelectLists and assign to dashboard model
            dashboard.CategorySelectList = await _newsService
                .GetCategorySelectListAsync(clientId, categoryId);
            dashboard.SubCategorySelectList = categoryId > 0
                ? await _newsService.GetSubCategorySelectListAsync(categoryId, subCategoryId)
                : new List<SelectListItem>();
            dashboard.WriterSelectList = await _newsService
                .GetWriterSelectListAsync(writerId);
            dashboard.PublicationSelectList = await _newsService
                .GetSourceSelectListAsync(
                    string.IsNullOrWhiteSpace(sourceType) ? "Publication" : sourceType,
                    publicationId);

            ViewBag.FilterTitle = title;
            ViewBag.FilterDateFrom = dateFrom;
            ViewBag.FilterDateTo = dateTo;
            ViewBag.FilterCategoryId = categoryId;
            ViewBag.FilterSubCategoryId = subCategoryId;
            ViewBag.FilterWriterId = writerId;
            ViewBag.FilterPublicationId = publicationId;
            ViewBag.FilterSourceType = sourceType;
            ViewBag.FilterPublished = published;
            ViewBag.SourceTypes = NewsOptions.SourceTypes;

            ViewBag.TotalAll = dashboard.TotalNews;
            ViewBag.TotalPublications = dashboard.Publications;
            ViewBag.TotalArticles = dashboard.Articles;
            ViewBag.TotalVideos = dashboard.Videos;
            ViewBag.TotalPR = dashboard.TotalPR;
            ViewBag.TotalAD = dashboard.TotalAD;
            ViewBag.TotalPublished = dashboard.TotalPublished;

            dashboard.Items = items.ToList();
            return View(dashboard);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _newsService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpGet]
        public IActionResult Create(int clientId)
        {
            ViewBag.ClientId = clientId;
            ViewBag.SourceTypes = NewsOptions.SourceTypes;
            return View("CreateSelect");
        }

        [HttpGet]
        public async Task<IActionResult> CreateNew(int clientId, string sourceType = "Publication")
        {
            var model = await BuildEmptyDtoAsync(clientId, sourceType, "New");
            return View("CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNew(ClientNewsDTO model)
        {
            model.NewsMode = "New";
            if (!ModelState.IsValid) { await PopulateDropdownsAsync(model); return View("CreateEdit", model); }
            var (success, message) = await _newsService.CreateAsync(model);
            if (!success) { ModelState.AddModelError(string.Empty, message); await PopulateDropdownsAsync(model); return View("CreateEdit", model); }
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateFromExisting(int clientId, string sourceType = "Publication")
        {
            var model = await BuildEmptyDtoAsync(clientId, sourceType, "Existing");
            return View("CreateEdit", model);
        }

        [HttpGet]
        public async Task<IActionResult> PrefillExisting(int newsId, int clientId)
        {
            var p = await _newsService.PrefillFromExistingNewsAsync(newsId, clientId);
            if (p == null) return Json(new { success = false, message = "News not found." });
            return Json(new
            {
                success = true,
                title = p.Title,
                date = p.Date.ToString("yyyy-MM-dd"),
                prValue = p.PRValue,
                adValue = p.ADValue,
                prOption = p.PROption,
                adOption = p.ADOption,
                articleBranding = p.ArticleBranding,
                headlineBranding = p.HeadlineBranding,
                pictureInArticle = p.pictureInArticle,
                generation = p.Generation,
                toning = p.Toning,
                translation = p.Translation
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromExisting(ClientNewsDTO model)
        {
            model.NewsMode = "Existing";
            if (!ModelState.IsValid) { await PopulateDropdownsAsync(model); return View("CreateEdit", model); }
            var (success, message) = await _newsService.CreateAsync(model);
            if (!success) { ModelState.AddModelError(string.Empty, message); await PopulateDropdownsAsync(model); return View("CreateEdit", model); }
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _newsService.GetByIdAsync(id);
            if (item == null) return NotFound();
            await PopulateDropdownsAsync(item);
            return View("CreateEdit", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientNewsDTO model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) { await PopulateDropdownsAsync(model); return View("CreateEdit", model); }
            var (success, message) = await _newsService.UpdateAsync(model);
            if (!success) { ModelState.AddModelError(string.Empty, message); await PopulateDropdownsAsync(model); return View("CreateEdit", model); }
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _newsService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int clientId)
        {
            var (success, message) = await _newsService.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { clientId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id, int clientId)
        {
            var (success, message) = await _newsService.PublishAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { clientId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id, int clientId)
        {
            var (success, message) = await _newsService.UnpublishAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { clientId });
        }

        // ── AJAX ──────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetSources(string sourceType, int selectedId = 0)
        {
            var list = await _newsService.GetSourceSelectListAsync(sourceType, selectedId);
            return Json(list.Select(i => new { value = i.Value, text = i.Text }));
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int parentId, int selectedId = 0)
        {
            var list = await _newsService.GetSubCategorySelectListAsync(parentId, selectedId);
            return Json(list.Select(i => new { value = i.Value, text = i.Text }));
        }

        [HttpGet]
        public async Task<IActionResult> GetExistingNews(string sourceType, int selectedId = 0)
        {
            var list = await _newsService.GetExistingNewsSelectListAsync(sourceType, selectedId);
            return Json(list.Select(i => new { value = i.Value, text = i.Text }));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task<ClientNewsDTO> BuildEmptyDtoAsync(int clientId, string sourceType, string mode)
        {
            var model = new ClientNewsDTO
            {
                ClientId = clientId,
                SourceType = sourceType,
                NewsMode = mode,
                Date = DateTime.Today,
                ArticleBranding = "N/A",
                HeadlineBranding = "N/A"
            };
            await PopulateDropdownsAsync(model);
            return model;
        }

        private async Task PopulateDropdownsAsync(ClientNewsDTO model)
        {
            model.SourceSelectList = await _newsService.GetSourceSelectListAsync(model.SourceType, model.publicationId);
            model.CategorySelectList = await _newsService.GetCategorySelectListAsync(model.ClientId, model.CategoryId);
            model.SubCategorySelectList = model.CategoryId > 0
                ? await _newsService.GetSubCategorySelectListAsync(model.CategoryId, model.SubCategoryId)
                : new List<SelectListItem>();
            model.WriterSelectList = await _newsService.GetWriterSelectListAsync(model.WriterId);
            model.ExistingNewsSelectList = model.NewsMode == "Existing"
                ? await _newsService.GetExistingNewsSelectListAsync(model.SourceType, model.ExistingNewsId ?? 0)
                : new List<SelectListItem>();
            model.BrandingSelectList = NewsOptions.BrandingSelectList();
            model.ToningSelectList = NewsOptions.ToningSelectList();
            model.TranslationSelectList = NewsOptions.TranslationSelectList();
        }
    }
}
