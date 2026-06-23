using EditorLogicLayer.Writer;
using EditorViewModelLayer.WriterViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,EditorWeb")]
    public class WriterController : Controller
    {
        private readonly IWriterService _writerService;

        public WriterController(IWriterService writerService)
            => _writerService = writerService;

        // GET: /writer
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        //[AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var writers = await _writerService.GetAllAsync();
            return View(writers);
        }

        // GET: /writer/Details/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,EditorWeb,Auditor")]
        //[AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var writer = await _writerService.GetByIdAsync(id);
            if (writer == null) return NotFound();
            return View(writer);
        }

        // GET: /writer/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        //[AllowAnonymous]
        public IActionResult Create() => View(new WriterDTO());

        // POST: /writer/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? state, WriterDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _writerService.CreateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /writer/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var writer = await _writerService.GetByIdAsync(id);
            if (writer == null) return NotFound();
            return View(writer);
        }

        // POST: /writer/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WriterDTO model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _writerService.UpdateAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /writer/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var writer = await _writerService.GetByIdAsync(id);
            if (writer == null) return NotFound();
            return View(writer);
        }

        // POST: /writer/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _writerService.DeleteAsync(id);

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }


        // POST: /writer/CreateQuick
        // Called via fetch(FormData) from a Bootstrap modal on other Create/Edit pages.
        // NOTE: [FromForm] (not [FromBody]) is required here — ValidateAntiForgeryToken
        // only reads the token from form fields, never from a JSON body.
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuick([FromForm] string writerName)
        {
            if (string.IsNullOrWhiteSpace(writerName))
                return BadRequest(new { error = "Writer name is required." });

            var model = new WriterDTO { WriterName = writerName.Trim() };
            var (success, message) = await _writerService.CreateAsync(model);

            if (!success)
                return BadRequest(new { error = message });

            // Re-fetch to get the generated Id
            var all = await _writerService.GetAllAsync();
            var created = all
                .OrderByDescending(w => w.Id)
                .FirstOrDefault(w => w.WriterName.Equals(
                    model.WriterName, StringComparison.OrdinalIgnoreCase));

            if (created == null)
                return BadRequest(new { error = "Writer created but could not be retrieved." });

            return Ok(new { id = created.Id, writerName = created.WriterName });
        }


    }
}
