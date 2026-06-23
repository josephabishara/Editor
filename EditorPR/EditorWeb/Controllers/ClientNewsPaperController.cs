using EditorLogicLayer.ClientNewsPaperLogic;
using EditorViewModelLayer.MediaViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    public class ClientNewsPaperController : Controller
    {
        private readonly IClientNewsPaperService _service;
        private readonly IWebHostEnvironment _env;

        public ClientNewsPaperController(
            IClientNewsPaperService service,
            IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        private static List<SelectListItem> ToSL(List<MediaSelectOption> src)
            => src.Select(o => new SelectListItem(o.Text, o.Value, o.Selected)).ToList();

        private static readonly string[] _allowedExt =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // ─────────────────────────────────────────────────────────────────────
        // IMAGE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private async Task<List<string>> SaveImagesAsync(IEnumerable<IFormFile> files)
        {
            var savedPaths = new List<string>();
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "newspapers");
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
                savedPaths.Add($"/uploads/newspapers/{uniqueName}");
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
            catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // INDEX
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Index(
            int clientId,
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            string? createdFrom = null,    
            string? createdTo = null,    
            int categoryId = 0,
            int subCategoryId = 0,
            //int writerId = 0,
            int publicationId = 0)
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
            // if (writerId > 0) items = items.Where(i => i.WriterId == writerId);
            if (publicationId > 0) items = items.Where(i => i.PublicationId == publicationId);

            if (DateTime.TryParse(createdFrom, out var cFrom))
                items = items.Where(i => i.CreatedAt.Date >= cFrom.Date);
            if (DateTime.TryParse(createdTo, out var cTo))
                items = items.Where(i => i.CreatedAt.Date <= cTo.Date);

            list.PublicationOptions = await _service.GetPublicationOptionsAsync(publicationId);
            list.CategoryOptions = await _service.GetCategoryOptionsAsync(clientId, categoryId);
            list.SubCategoryOptions = categoryId > 0
                ? await _service.GetSubCategoryOptionsAsync(categoryId, subCategoryId)
                : new List<MediaSelectOption>();
           // list.WriterOptions = await _service.GetWriterOptionsAsync(writerId);
            list.Items = items.ToList();

            ViewBag.ClientId = clientId;
            ViewBag.FilterTitle = title;
            ViewBag.FilterDateFrom = dateFrom;
            ViewBag.FilterDateTo = dateTo;
            ViewBag.FilterCategoryId = categoryId;
            ViewBag.FilterSubId = subCategoryId;
            //ViewBag.FilterWriterId = writerId;
            ViewBag.FilterPubId = publicationId;
            ViewBag.FilterCreatedFrom = createdFrom;
            ViewBag.FilterCreatedTo = createdTo;

            return View(list);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DETAILS
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Create(int clientId)
        {
            if (clientId <= 0) return RedirectToAction("Index", "Dashboard");
            var model = new ClientNewsPaperDTO { ClientId = clientId, Date = DateTime.Today };
            await PopulateDropdownsAsync(model);
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ClientNewsPaperDTO model,
            List<IFormFile>? imageFiles,
            string? removedImages,
            bool? addChild,
            int? removeChild,
            bool? saveAndDuplicate)   // ← new parameter
        {
            if (addChild == true)
            {
                model.Children.Add(new ChildNewsPaperDTO { Date = DateTime.Today });
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            if (removeChild.HasValue)
            {
                var idx = removeChild.Value;
                if (idx >= 0 && idx < model.Children.Count)
                    model.Children.RemoveAt(idx);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            if (imageFiles != null && imageFiles.Count > 0)
            {
                var paths = await SaveImagesAsync(imageFiles);
                if (paths.Any())
                    model.Images = System.Text.Json.JsonSerializer.Serialize(paths);
            }

            var (success, message, newId) = await _service.CreateAsync(model);  // ← destructure NewId
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // ── Save & Duplicate ──────────────────────────────────────────────
            if (saveAndDuplicate == true)
            {
                var (dupSuccess, dupMessage, dupId) = await _service.DuplicateAsync(newId);
                if (dupSuccess)
                {
                    TempData["Success"] = "Saved and duplicated. Now editing the duplicate copy.";
                    return RedirectToAction(nameof(Edit), new { id = dupId });
                }
                // Duplicate failed — original was saved, fall back to normal redirect
                TempData["Success"] = message;
                TempData["Error"] = $"Save succeeded but duplication failed: {dupMessage}";
                return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Create), new { clientId = model.ClientId });
        }




        // ─────────────────────────────────────────────────────────────────────
        // EDIT
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            await PopulateDropdownsAsync(item);
            return View(item);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ClientNewsPaperDTO model,        // ← was ClientArticleDTO
            List<IFormFile>? imageFiles,
            string? removedImages,
            bool? addChild,
            int? removeChild,
            bool? saveAndDuplicate)
        {
            if (id != model.Id) return BadRequest();

            if (addChild == true)
            {
                model.Children.Add(new ChildNewsPaperDTO { Date = DateTime.Today });  // ← was ChildArticleDTO
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            if (removeChild.HasValue)
            {
                var idx = removeChild.Value;
                if (idx >= 0 && idx < model.Children.Count)
                    model.Children.RemoveAt(idx);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var existingPaths = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.Images))
            {
                try { existingPaths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(model.Images) ?? new(); }
                catch { existingPaths = new(); }
            }

            if (!string.IsNullOrWhiteSpace(removedImages))
            {
                List<string> toRemove;
                try { toRemove = System.Text.Json.JsonSerializer.Deserialize<List<string>>(removedImages) ?? new(); }
                catch { toRemove = new(); }
                foreach (var path in toRemove) { DeleteImageFile(path); existingPaths.Remove(path); }
            }

            if (imageFiles != null && imageFiles.Count > 0)
            {
                var newPaths = await SaveImagesAsync(imageFiles);
                existingPaths.AddRange(newPaths);
            }

            model.Images = existingPaths.Any()
                ? System.Text.Json.JsonSerializer.Serialize(existingPaths)
                : null;

            var (success, message) = await _service.UpdateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            if (saveAndDuplicate == true)
            {
                var (dupSuccess, dupMessage, dupId) = await _service.DuplicateAsync(model.Id);
                if (dupSuccess)
                {
                    TempData["Success"] = "Saved and duplicated. Now editing the duplicate copy.";
                    return RedirectToAction(nameof(Edit), new { id = dupId });
                }
                TempData["Success"] = message;
                TempData["Error"] = $"Save succeeded but duplication failed: {dupMessage}";
                return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), new { clientId = model.ClientId });
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────────────────────────────

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
            // Delete image files from disk on soft-delete
            var item = await _service.GetByIdAsync(id);
            if (item != null && !string.IsNullOrWhiteSpace(item.Images))
            {
                try
                {
                    var paths = JsonSerializer.Deserialize<List<string>>(item.Images) ?? new();
                    foreach (var p in paths) DeleteImageFile(p);
                }
                catch { }
            }

            var (success, message) = await _service.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { clientId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids, int clientId)
        {
            if (ids == null || !ids.Any())
            {
                TempData["Error"] = "No newspapers selected.";
                return RedirectToAction(nameof(Index), new { clientId });
            }

            foreach (var id in ids)
            {
                var item = await _service.GetByIdAsync(id);
                if (item != null && !string.IsNullOrWhiteSpace(item.Images))
                {
                    try
                    {
                        var paths = JsonSerializer.Deserialize<List<string>>(item.Images) ?? new();
                        foreach (var p in paths) DeleteImageFile(p);
                    }
                    catch { }
                }
            }

            var (success, message) = await _service.BulkDeleteAsync(ids);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { clientId });
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUBLISH
        // ─────────────────────────────────────────────────────────────────────

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


        // POST: /ClientNewsPaper/Duplicate
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
            return RedirectToAction(nameof(Edit), new { id = newId });
        }


        // ─────────────────────────────────────────────────────────────────────
        // AJAX
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int parentId)
        {
            var opts = await _service.GetSubCategoryOptionsAsync(parentId);
            return Json(opts.Select(o => new { value = o.Value, text = o.Text }));
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicationAutoFill(int publicationId, int clientId)
        {
            var data = await _service.GetPublicationAutoFillAsync(publicationId, clientId);
            return Json(data);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private async Task PopulateDropdownsAsync(ClientNewsPaperDTO model)
        {
            model.PublicationOptions = await _service.GetPublicationOptionsAsync(model.PublicationId);
            model.CategoryOptions = await _service.GetCategoryOptionsAsync(model.ClientId, model.CategoryId);
            model.SubCategoryOptions = model.CategoryId > 0
                ? await _service.GetSubCategoryOptionsAsync(model.CategoryId, model.SubCategoryId)
                : new List<MediaSelectOption>();
            model.WriterOptions = await _service.GetWriterOptionsAsync(model.WriterId);
            model.BrandingOptions = MediaOptions.BrandingList();
            model.ToningOptions = MediaOptions.ToningList();
            model.YesNoOptions = MediaOptions.YesNoList();
            model.GenerationOptions = MediaOptions.GenerationList();

            if (string.IsNullOrEmpty(model.ArticleBranding)) model.ArticleBranding = "Branded";
            if (string.IsNullOrEmpty(model.HeadlineBranding)) model.HeadlineBranding = "Branded";
            if (string.IsNullOrEmpty(model.Toning)) model.Toning = "Neutral";
            if (string.IsNullOrEmpty(model.PictureinArticle)) model.PictureinArticle = "Yes";
            if (string.IsNullOrEmpty(model.Generation)) model.Generation = "Not Generated";
        }
    }
}
