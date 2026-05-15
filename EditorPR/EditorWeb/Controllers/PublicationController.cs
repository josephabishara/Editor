using EditorLogicLayer.Publication;
using EditorViewModelLayer.PublicationViewModel;
using EditorViewModelLayer.WebsiteViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    public class PublicationController : Controller
    {
        private readonly IPublicationService _publicationService;

        public PublicationController(IPublicationService publicationService)
            => _publicationService = publicationService;

        // GET: /Publication
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Index()
        {
            var publications = await _publicationService.GetAllAsync();
            return View(publications);
        }

        // GET: /Publication/Details/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var publication = await _publicationService.GetByIdAsync(id);
            if (publication == null) return NotFound();
            return View(publication);
        }

        // GET: /Publication/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create() => View(new PublicationDTO());

        // POST: /Publication/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PublicationDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _publicationService.CreateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Publication/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var publication = await _publicationService.GetByIdAsync(id);
            if (publication == null) return NotFound();
            return View(publication);
        }

        // POST: /Publication/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PublicationDTO model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _publicationService.UpdateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Publication/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var publication = await _publicationService.GetByIdAsync(id);
            if (publication == null) return NotFound();
            return View(publication);
        }

        // POST: /Publication/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _publicationService.DeleteAsync(id);

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // EXPORT
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Website/ExportToExcel
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> ExportToExcel()
        {
            var publications = await _publicationService.GetAllAsync();
            var fileBytes = _publicationService.ExportToExcel(publications);
            var fileName = $"Publications_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ══════════════════════════════════════════════════════════════════════
        // IMPORT
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Publication/Import
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Import() => View();

        // POST: /Publication/Import
        // ✅ Clean architecture: IFormFile lives ONLY here in the Controller
        //    Converted to Stream before calling service — LogicLayer never sees IFormFile
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
                await _publicationService.ImportFromExcelAsync(stream, file.FileName);

            if (!success)
            {
                TempData["Error"] = message;
                return View();
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Publication/DownloadTemplate
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DownloadTemplate()
        {
            var bytes = _publicationService.ExportToExcel(Enumerable.Empty<PublicationDTO>());
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Publications_ImportTemplate.xlsx");
        }
    }
}
