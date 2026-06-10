using EditorLogicLayer.ClientArticleLogic;
using EditorViewModelLayer.MediaViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    public class ClientArticleController : Controller
    {
        private readonly IClientArticleService _service;
        private readonly IWebHostEnvironment _env;

        public ClientArticleController(
            IClientArticleService service,
            IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        

        // ── Allowed image extensions ───────────────────────────────────────────
        private static readonly string[] _allowedExt =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static List<SelectListItem> ToSL(List<MediaSelectOption> src)
            => src.Select(o => new SelectListItem(o.Text, o.Value, o.Selected)).ToList();

        // ── INDEX ──────────────────────────────────────────────────────────────

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
            int websiteId = 0)
        {
            if (clientId <= 0) return RedirectToAction("Index", "Dashboard");

            var list = await _service.GetListAsync(clientId);
            var items = list.Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
                items = items.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(dateFrom, out var from))
                items = items.Where(i => i.Date.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to))
                items = items.Where(i => i.Date.Date <= to.Date);
            if (categoryId > 0) items = items.Where(i => i.CategoryId == categoryId);
            if (subCategoryId > 0) items = items.Where(i => i.SubCategoryId == subCategoryId);
            if (writerId > 0) items = items.Where(i => i.WriterId == writerId);
            if (websiteId > 0) items = items.Where(i => i.WebsiteId == websiteId);

            list.WebsiteOptions = await _service.GetWebsiteOptionsAsync(websiteId);
            list.CategoryOptions = await _service.GetCategoryOptionsAsync(clientId, categoryId);
            list.SubCategoryOptions = categoryId > 0
                ? await _service.GetSubCategoryOptionsAsync(categoryId, subCategoryId)
                : new List<MediaSelectOption>();
            list.WriterOptions = await _service.GetWriterOptionsAsync(writerId);
            list.Items = items.ToList();

            ViewBag.ClientId = clientId;
            ViewBag.FilterTitle = title;
            ViewBag.FilterDateFrom = dateFrom;
            ViewBag.FilterDateTo = dateTo;
            ViewBag.FilterCategoryId = categoryId;
            ViewBag.FilterSubId = subCategoryId;
            ViewBag.FilterWriterId = writerId;
            ViewBag.FilterWebsiteId = websiteId;

            return View(list);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int clientId)
        {
            if (clientId <= 0) return RedirectToAction("Index", "Dashboard");
            var model = new ClientArticleDTO { ClientId = clientId, Date = DateTime.Today };
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(ClientArticleDTO model)
        //{
        //    if (!ModelState.IsValid) { await PopulateDropdownsAsync(model); return View(model); }
        //    var (success, message) = await _service.CreateAsync(model);
        //    if (!success) { ModelState.AddModelError(string.Empty, message); await PopulateDropdownsAsync(model); return View(model); }
        //    TempData["Success"] = message;
        //    return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
           ClientArticleDTO model,
           List<IFormFile>? imageFiles)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // ── Upload new images ──────────────────────────────────────────
            if (imageFiles != null && imageFiles.Count > 0)
            {
                var paths = await SaveImagesAsync(imageFiles);
                if (paths.Any())
                    model.Images = JsonSerializer.Serialize(paths);
            }

            var (success, message) = await _service.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Create), new { clientId = model.ClientId });
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            await PopulateDropdownsAsync(item);
            return View(item);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, ClientArticleDTO model)
        //{
        //    if (id != model.Id) return BadRequest();
        //    if (!ModelState.IsValid) { await PopulateDropdownsAsync(model); return View(model); }
        //    var (success, message) = await _service.UpdateAsync(model);
        //    if (!success) { ModelState.AddModelError(string.Empty, message); await PopulateDropdownsAsync(model); return View(model); }
        //    TempData["Success"] = message;
        //    return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
          int id,
          ClientArticleDTO model,
          List<IFormFile>? imageFiles,
          string? removedImages)   // posted as hidden field from the view
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // ── 1. Start from existing paths ───────────────────────────────
            var existingPaths = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.Images))
            {
                try { existingPaths = JsonSerializer.Deserialize<List<string>>(model.Images) ?? new(); }
                catch { existingPaths = new(); }
            }

            // ── 2. Remove paths the user deleted in the UI ─────────────────
            if (!string.IsNullOrWhiteSpace(removedImages))
            {
                List<string> toRemove;
                try { toRemove = JsonSerializer.Deserialize<List<string>>(removedImages) ?? new(); }
                catch { toRemove = new(); }

                foreach (var path in toRemove)
                {
                    DeleteImageFile(path);
                    existingPaths.Remove(path);
                }
            }

            // ── 3. Upload new images and append ────────────────────────────
            if (imageFiles != null && imageFiles.Count > 0)
            {
                var newPaths = await SaveImagesAsync(imageFiles);
                existingPaths.AddRange(newPaths);
            }

            // ── 4. Persist final list ──────────────────────────────────────
            model.Images = existingPaths.Any()
                ? JsonSerializer.Serialize(existingPaths)
                : null;

            var (success, message) = await _service.UpdateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int clientId)
        {
            var (success, message) = await _service.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { clientId });
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int parentId)
        {
            var opts = await _service.GetSubCategoryOptionsAsync(parentId);
            return Json(opts.Select(o => new { value = o.Value, text = o.Text }));
        }

        [HttpGet]
        public async Task<IActionResult> GetWebsiteAutoFill(int websiteId, int clientId)
        {
            var data = await _service.GetWebsiteAutoFillAsync(websiteId, clientId);
            return Json(data);
        }
        // ── PUBLISH ────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id, int clientId)
        {
            var (success, message) = await _service.PublishAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { clientId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id, int clientId)
        {
            var (success, message) = await _service.UnpublishAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { clientId });
        }
        //private async Task PopulateDropdownsAsync(ClientArticleDTO model)
        //{
        //    model.WebsiteOptions = await _service.GetWebsiteOptionsAsync(model.WebsiteId);
        //    model.CategoryOptions = await _service.GetCategoryOptionsAsync(model.ClientId, model.CategoryId);
        //    model.SubCategoryOptions = model.CategoryId > 0
        //        ? await _service.GetSubCategoryOptionsAsync(model.CategoryId, model.SubCategoryId)
        //        : new List<MediaSelectOption>();
        //    model.WriterOptions = await _service.GetWriterOptionsAsync(model.WriterId);
        //    model.BrandingOptions = MediaOptions.BrandingList();
        //    model.ArticleBranding = "Branded";
        //    model.HeadlineBranding = "Branded";
        //    model.ToningOptions = MediaOptions.ToningList();
        //    model.Toning = "Neutral";
        //    model.YesNoOptions = MediaOptions.YesNoList();
        //    model.PictureinArticle = "Yes";
        //    model.GenerationOptions = MediaOptions.GenerationList();
        //    model.Generation = "Generation";
        //}


        // POST: /ClientArticle/Duplicate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int id, int clientId)
        {
            var (success, message, newId) = await _service.DuplicateAsync(id);

            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Index), new { clientId });
            }

            TempData["Success"] = message;
            // Redirect straight to the duplicated record so the user can edit it
            return RedirectToAction(nameof(Edit), new { id = newId });
        }

        private async Task PopulateDropdownsAsync(ClientArticleDTO model)
        {
            model.WebsiteOptions = await _service.GetWebsiteOptionsAsync(model.WebsiteId);
            model.CategoryOptions = await _service.GetCategoryOptionsAsync(model.ClientId, model.CategoryId);
            model.SubCategoryOptions = model.CategoryId > 0
                ? await _service.GetSubCategoryOptionsAsync(model.CategoryId, model.SubCategoryId)
                : new List<MediaSelectOption>();
            model.WriterOptions = await _service.GetWriterOptionsAsync(model.WriterId);
            model.BrandingOptions = MediaOptions.BrandingList();
            model.ToningOptions = MediaOptions.ToningList();
            model.YesNoOptions = MediaOptions.YesNoList();
            model.GenerationOptions = MediaOptions.GenerationList();

            // Only set defaults if not already populated (preserve user input on failed POST)
            if (string.IsNullOrEmpty(model.ArticleBranding)) model.ArticleBranding = "Branded";
            if (string.IsNullOrEmpty(model.HeadlineBranding)) model.HeadlineBranding = "Branded";
            if (string.IsNullOrEmpty(model.Toning)) model.Toning = "Neutral";
            if (string.IsNullOrEmpty(model.PictureinArticle)) model.PictureinArticle = "Yes";
            if (string.IsNullOrEmpty(model.Generation)) model.Generation = "Generation";
        }
        private async Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> files)
        {
            var savedPaths = new List<string>();
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "articles");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExt.Contains(ext)) continue;

                var uniqueName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadsFolder, uniqueName);

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                savedPaths.Add($"/uploads/articles/{uniqueName}");
            }

            return savedPaths;
        }

        private void DeleteImageFile(string relativePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativePath)) return;
                var full = Path.Combine(
                    _env.WebRootPath,
                    relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(full))
                    System.IO.File.Delete(full);
            }
            catch { /* swallow — non-critical */ }
        }

    }

}
