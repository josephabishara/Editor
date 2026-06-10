using EditorLogicLayer.Client;
using EditorViewModelLayer.ClientViewModel;
using EditorViewModelLayer.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;
       
        public ClientController(IClientService clientService)
            => _clientService = clientService;

        // ══════════════════════════════════════════════════════════════════════
        // CLIENT CRUD
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Client
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllAsync();
            return View(clients);
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> All()
        {
            var clients = await _clientService.GetAllAsync();
            return View(clients);
        }
        // GET: /Client/Details/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientService.GetWithAssistantsAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> Dashboard(int id)
        {
            var client = await _clientService.GetWithAssistantsAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // GET: /Client/Create
        [HttpGet]
        public IActionResult Create() => View(new ClientDTO());

        // POST: /Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientDTO model , IFormFile? photoFile)
        {
            if (!ModelState.IsValid) return View(model);
            if (photoFile != null && photoFile.Length > 0)
            {
                model.PhotoFile = new UploadFileDTO
                {
                    FileName = photoFile.FileName,
                    FileStream = photoFile.OpenReadStream(),
                    ContentType = photoFile.ContentType,
                    Length = photoFile.Length
                };
            }
            var (success, message) = await _clientService.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Client/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: /Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientDTO model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _clientService.UpdateAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Client/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: /Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _clientService.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Client/EditCategories/5
        [HttpGet]
        public async Task<IActionResult> EditCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            var categories = await _clientService.GetClientCategoriesAsync(id);

            var model = new UpdateClientCategoriesDTO
            {
                CustomerId = id,
                Categories = categories.ToList()
            };

            ViewBag.ClientName = client.Name;
            return View(model);
        }

        // POST: /Client/EditCategories
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategories(UpdateClientCategoriesDTO model)
        {
            var (success, message) = await _clientService.UpdateClientCategoriesAsync(model);
            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(EditCategories), new { id = model.CustomerId });
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Details), new { id = model.CustomerId });
        }


        // GET: /Client/EditPublicationCategories/5
        [HttpGet]
        public async Task<IActionResult> EditPublicationCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            var categories = await _clientService.GetClientPublicationCategoriesAsync(id);
            var model = new UpdateClientPublicationCategoriesDTO
            {
                CustomerId = id,
                Categories = categories.ToList()
            };

            ViewBag.ClientName = client.Name;
            return View(model);
        }

        // POST: /Client/EditPublicationCategories
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPublicationCategories(UpdateClientPublicationCategoriesDTO model)
        {
            var (success, message) = await _clientService.UpdateClientPublicationCategoriesAsync(model);
            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(EditPublicationCategories), new { id = model.CustomerId });
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Details), new { id = model.CustomerId });
        }

        // GET: /Client/EditChannelCategories/5
        [HttpGet]
        public async Task<IActionResult> EditChannelCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            var categories = await _clientService.GetClientChannelCategoriesAsync(id);
            var model = new UpdateClientChannelCategoriesDTO
            {
                CustomerId = id,
                Categories = categories.ToList()
            };

            ViewBag.ClientName = client.Name;
            return View(model);
        }

        // POST: /Client/EditChannelCategories
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChannelCategories(UpdateClientChannelCategoriesDTO model)
        {
            var (success, message) = await _clientService.UpdateClientChannelCategoriesAsync(model);
            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(EditChannelCategories), new { id = model.CustomerId });
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Details), new { id = model.CustomerId });
        }


        // ══════════════════════════════════════════════════════════════════════
        // PHOTO
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Client/ChangePhoto/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> ChangePhoto(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client); // passes ClientDTO — view shows current photo + upload form
        }

        // POST: /Client/ChangePhoto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> ChangePhoto(int id, IFormFile? photoFile)
        {
            try
            {
                if (photoFile == null || photoFile.Length == 0)
                {
                    ModelState.AddModelError("photoFile", "Please select a photo to upload.");
                    // Reload client to redisplay the current photo in the view
                    var client = await _clientService.GetByIdAsync(id);
                    return View(client);
                }

                var uploadDto = new UploadFileDTO
                {
                    FileName = photoFile.FileName,
                    FileStream = photoFile.OpenReadStream(),
                    ContentType = photoFile.ContentType,
                    Length = photoFile.Length
                };

                var (success, message) = await _clientService.ChangePhotoAsync(id, uploadDto);
                if (!success)
                {
                    ModelState.AddModelError("photoFile", message);
                    var client = await _clientService.GetByIdAsync(id);
                    return View(client);
                }
                TempData["Success"] = message;
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            
           
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Client/ChangePhoto/5
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> ChangeAssistantPhoto(int id)
        {
            var assistant = await _clientService.GetAssistantByIdAsync(id);
            if (assistant == null) return NotFound();
            return View(assistant); // passes ClientDTO — view shows current photo + upload form
        }

        // POST: /Client/ChangePhoto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> ChangeAssistantPhoto(int id, IFormFile? photoFile)
        {
            try
            {
                if (photoFile == null || photoFile.Length == 0)
                {
                    ModelState.AddModelError("photoFile", "Please select a photo to upload.");
                    // Reload client to redisplay the current photo in the view
                    var client = await _clientService.GetByIdAsync(id);
                    return View(client);
                }

                var uploadDto = new UploadFileDTO
                {
                    FileName = photoFile.FileName,
                    FileStream = photoFile.OpenReadStream(),
                    ContentType = photoFile.ContentType,
                    Length = photoFile.Length
                };

                var (success, message) = await _clientService.ChangeAssistantPhotoAsync(id, uploadDto);
                if (!success)
                {
                    ModelState.AddModelError("photoFile", message);
                    var assistant = await _clientService.GetAssistantByIdAsync(id); 
                    if (assistant == null) return NotFound();
                    var client = await _clientService.GetByIdAsync(assistant.ClientId);
                    return View(client);
                }
                TempData["Success"] = message;
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }


            return RedirectToAction(nameof(Details), new { id });
        }

        // ══════════════════════════════════════════════════════════════════════
        // ASSISTANT CRUD (nested under Client)
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Client/CreateAssistant?clientId=5
        [HttpGet]
        public async Task<IActionResult> CreateAssistant(int clientId)
        {
            var client = await _clientService.GetByIdAsync(clientId);
            if (client == null) return NotFound();

            return View(new AssistantDTO
            {
                ClientId = clientId,
                ClientName = client.Name
            });
        }

        // POST: /Client/CreateAssistant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssistant(AssistantDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _clientService.CreateAssistantAsync(model); 
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Details), new { id = model.ClientId });
        }

        // GET: /Client/EditAssistant/5
        [HttpGet]
        public async Task<IActionResult> EditAssistant(int id)
        {
            var assistant = await _clientService.GetAssistantByIdAsync(id);  
            if (assistant == null) return NotFound();
            return View(assistant);
        }

        // POST: /Client/EditAssistant/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssistant(int id, AssistantDTO model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _clientService.UpdateAssistantAsync(model); // FIXED: was _assistantService (null)
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Details), new { id = model.ClientId });
        }

        // POST: /Client/DeleteAssistant
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAssistant(int id, int clientId)
        {
            var (success, message) = await _clientService.DeleteAssistantAsync(id); // FIXED: was _assistantService (null)
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id = clientId });
        }


        // ── Website Categories Excel ──────────────────────────────────────────────────

        // GET: /Client/ExportWebsiteCategories/5
        [HttpGet]
        public async Task<IActionResult> ExportWebsiteCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            var categories = await _clientService.GetClientCategoriesAsync(id);
            var bytes = _clientService.ExportWebsiteCategoriesToExcel(categories, client.Name);
            var fileName = $"WebsiteCategories_{client.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // GET: /Client/ImportWebsiteCategories/5
        [HttpGet]
        public async Task<IActionResult> ImportWebsiteCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            ViewBag.ClientId = id;
            ViewBag.ClientName = client.Name;
            return View();
        }

        // POST: /Client/ImportWebsiteCategories/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportWebsiteCategories(int id, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file.";
                return RedirectToAction(nameof(ImportWebsiteCategories), new { id });
            }

            using var stream = file.OpenReadStream();
            var (success, message, _) =
                await _clientService.ImportWebsiteCategoriesFromExcelAsync(id, stream, file.FileName);

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(
                success ? nameof(EditCategories) : nameof(ImportWebsiteCategories),
                new { id });
        }


        // ── Publication Categories Excel ──────────────────────────────────────────────

        // GET: /Client/ExportPublicationCategories/5
        [HttpGet]
        public async Task<IActionResult> ExportPublicationCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            var categories = await _clientService.GetClientPublicationCategoriesAsync(id);
            var bytes = _clientService.ExportPublicationCategoriesToExcel(categories, client.Name);
            var fileName = $"PublicationCategories_{client.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // GET: /Client/ImportPublicationCategories/5
        [HttpGet]
        public async Task<IActionResult> ImportPublicationCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            ViewBag.ClientId = id;
            ViewBag.ClientName = client.Name;
            return View();
        }

        // POST: /Client/ImportPublicationCategories/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportPublicationCategories(int id, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file.";
                return RedirectToAction(nameof(ImportPublicationCategories), new { id });
            }

            using var stream = file.OpenReadStream();
            var (success, message, _) =
                await _clientService.ImportPublicationCategoriesFromExcelAsync(id, stream, file.FileName);

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(
                success ? nameof(EditPublicationCategories) : nameof(ImportPublicationCategories),
                new { id });
        }


        // ── Channel Categories Excel ──────────────────────────────────────────────────

        // GET: /Client/ExportChannelCategories/5
        [HttpGet]
        public async Task<IActionResult> ExportChannelCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            var categories = await _clientService.GetClientChannelCategoriesAsync(id);
            var bytes = _clientService.ExportChannelCategoriesToExcel(categories, client.Name);
            var fileName = $"ChannelCategories_{client.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // GET: /Client/ImportChannelCategories/5
        [HttpGet]
        public async Task<IActionResult> ImportChannelCategories(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            ViewBag.ClientId = id;
            ViewBag.ClientName = client.Name;
            return View();
        }

        // POST: /Client/ImportChannelCategories/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportChannelCategories(int id, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file.";
                return RedirectToAction(nameof(ImportChannelCategories), new { id });
            }

            using var stream = file.OpenReadStream();
            var (success, message, _) =
                await _clientService.ImportChannelCategoriesFromExcelAsync(id, stream, file.FileName);

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(
                success ? nameof(EditChannelCategories) : nameof(ImportChannelCategories),
                new { id });
        }


    }
}
