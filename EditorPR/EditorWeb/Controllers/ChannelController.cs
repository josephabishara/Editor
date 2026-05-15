using EditorLogicLayer.Channel;
using EditorViewModelLayer.ChannelViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    public class ChannelController : Controller
    {
        private readonly IChannelService _channelService;

        public ChannelController(IChannelService channelService)
            => _channelService = channelService;

        // ══════════════════════════════════════════════════════════════════════
        // CRUD
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Channel
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Index()
        {
            var channels = await _channelService.GetAllAsync();
            return View(channels);
        }

        // GET: /Channel/Details/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var channel = await _channelService.GetByIdAsync(id);
            if (channel == null) return NotFound();
            return View(channel);
        }

        // GET: /Channel/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create() => View(new ChannelDTO());

        // POST: /Channel/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChannelDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _channelService.CreateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Channel/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var channel = await _channelService.GetByIdAsync(id);
            if (channel == null) return NotFound();
            return View(channel);
        }

        // POST: /Channel/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChannelDTO model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _channelService.UpdateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Channel/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var channel = await _channelService.GetByIdAsync(id);
            if (channel == null) return NotFound();
            return View(channel);
        }

        // POST: /Channel/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _channelService.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════════════════════════════════
        // EXPORT
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Channel/ExportToExcel
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        public async Task<IActionResult> ExportToExcel()
        {
            var channels = await _channelService.GetAllAsync();
            var fileBytes = _channelService.ExportToExcel(channels);
            var fileName = $"Channels_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ══════════════════════════════════════════════════════════════════════
        // IMPORT
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Channel/Import
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Import() => View();

        // POST: /Channel/Import
        // ✅ IFormFile converted to Stream here — LogicLayer never sees IFormFile
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
                await _channelService.ImportFromExcelAsync(stream, file.FileName);

            if (!success)
            {
                TempData["Error"] = message;
                return View();
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Channel/DownloadTemplate
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DownloadTemplate()
        {
            var bytes = _channelService.ExportToExcel(Enumerable.Empty<ChannelDTO>());
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Channels_ImportTemplate.xlsx");
        }
    }
}
