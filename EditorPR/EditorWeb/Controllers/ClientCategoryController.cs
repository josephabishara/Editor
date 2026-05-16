using EditorLogicLayer.Client;
using EditorViewModelLayer.ClientViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EditorWeb.Controllers
{
    public class ClientCategoryController : Controller
    {
        private readonly IClientCategoryService _service;

        public ClientCategoryController(IClientCategoryService service)
            => _service = service;

        // GET: /ClientCategory?ClientId=5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Index(int ClientId)
        {
            var categories = await _service.GetRootCategoriesAsync(ClientId);
            ViewBag.ClientId = ClientId;
            return View(categories);
        }

        // GET: /ClientCategory/Details/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // GET: /ClientCategory/Create?ClientId=5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(int ClientId, int? parentId)
        {
            var model = new ClientCategoryDTO
            {
                ClientId = ClientId,
                ParentCategory = parentId,
                Status = "Active"
            };

            await PopulateParentDropdown(ClientId, null);
            return View(model);
        }

        // POST: /ClientCategory/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCategoryDTO model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateParentDropdown(model.ClientId, null);
                return View(model);
            }

            var (success, message) = await _service.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateParentDropdown(model.ClientId, null);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), new { ClientId = model.ClientId });
        }

        // GET: /ClientCategory/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();

            await PopulateParentDropdown(category.ClientId, id);
            return View(category);
        }

        // POST: /ClientCategory/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientCategoryDTO model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateParentDropdown(model.ClientId, id);
                return View(model);
            }

            var (success, message) = await _service.UpdateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateParentDropdown(model.ClientId, id);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), new { ClientId = model.ClientId });
        }

        // GET: /ClientCategory/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /ClientCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int ClientId)
        {
            var (success, message) = await _service.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index), new { ClientId });
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private async Task PopulateParentDropdown(int ClientId, int? excludeId)
        {
            var allCategories = await _service.GetByClientAsync(ClientId);
            // Exclude the category itself to prevent circular reference
            if (excludeId.HasValue)
                allCategories = allCategories.Where(c => c.Id != excludeId.Value);

            ViewBag.ParentCategories = allCategories;
        }
    }
}

