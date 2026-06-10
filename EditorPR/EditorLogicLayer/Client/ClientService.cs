    using EditorEntitiesLayer.Entities;
using EditorLogicLayer.Helpers;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.ClientViewModel;
using EditorViewModelLayer.General;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace EditorLogicLayer.Client
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepo;
        private readonly IAssistantRepository _assistantRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IWebsiteCustomerCategoryRepository _categoryRepo;
        private readonly IWebsiteRepository _websiteRepo;
        private readonly IPublicationCustomerCategoryRepository _publicationCategoryRepo;
        private readonly IChannelCustomerCategoryRepository _channelCategoryRepo;
        private readonly IPublicationRepository _publicationRepo;
        private readonly IChannelRepository _channelRepo;

        public ClientService(
            IClientRepository clientRepo,
            IAssistantRepository assistantRepo,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IWebsiteCustomerCategoryRepository categoryRepo,
            IWebsiteRepository websiteRepo,
            IPublicationCustomerCategoryRepository publicationCategoryRepo,
            IChannelCustomerCategoryRepository channelCategoryRepo,
            IPublicationRepository publicationRepo,
            IChannelRepository channelRepo)
        {
            _clientRepo = clientRepo;
            _assistantRepo = assistantRepo;
            _userManager = userManager;
            _env = env;
            _categoryRepo = categoryRepo;
            _websiteRepo = websiteRepo;
            _publicationCategoryRepo = publicationCategoryRepo;
            _channelCategoryRepo = channelCategoryRepo;
            _publicationRepo = publicationRepo;
            _channelRepo = channelRepo;
        }

        // ── Client CRUD ────────────────────────────────────────────────────────

        public async Task<IEnumerable<ClientDTO>> GetAllAsync()
        {
            var clients = await _clientRepo.GetActiveClientsAsync();
            return clients.Select(MapToDTO);
        }

        public async Task<ClientDTO?> GetByIdAsync(int id)
        {
            var client = await _clientRepo.GetByIdAsync(id);
            return client == null ? null : MapToDTO(client);
        }

        public async Task<ClientDTO?> GetWithAssistantsAsync(int id)
        {
            var client = await _clientRepo.GetClientWithAssistantsAsync(id);
            if (client == null) return null;

            var dto = MapToDTO(client);
            dto.AssistantList = client.AssistantList.Select(MapAssistantToDTO).ToList();

            dto.WebsiteCategories = (await _categoryRepo.GetByClientIdAsync(id))
                                      .Select(MapCategoryToDTO).ToList();
            dto.PublicationCategories = (await _publicationCategoryRepo.GetByClientIdAsync(id))
                                          .Select(MapPublicationCategoryToDTO).ToList();
            dto.ChannelCategories = (await _channelCategoryRepo.GetByClientIdAsync(id))
                                      .Select(MapChannelCategoryToDTO).ToList();
            return dto;
        }

        public async Task<(bool Success, string Message)> CreateAsync(ClientDTO model)
        {
            if (await _clientRepo.EmailExistsAsync(model.Email))
                return (false, "A client with this email already exists.");

            if (await _clientRepo.UsernameExistsAsync(model.Username))
                return (false, "A client with this username already exists.");

            if (string.IsNullOrWhiteSpace(model.Password))
                return (false, "Password is required when creating a client.");

            var user = new ApplicationUser
            {
                FullName = model.Name,
                UserName = model.Email,
                Email = model.Email,

                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var userResult = await _userManager.CreateAsync(user, model.Password);
            if (!userResult.Succeeded)
            {
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                return (false, errors);
            }

            await _userManager.AddToRoleAsync(user, "Client");

            var entity = MapToEntity(model);
            entity.ApplicationUserId = user.Id;
            entity.CreatedAt = DateTime.UtcNow; // BUG 3: was entity.CreatedAt — field is CreatedDate per BaseEntity

            await _clientRepo.AddAsync(entity);

            // 3. Auto-generate WebsiteCustomerCategory rows from ALL active websites
            //    Each row inherits the website's current MediaTier as default
            var websites = await _websiteRepo.GetActiveWebsitesAsync();
            var categories = websites.Select(w => new WebsiteCustomerCategory
            {
                CustomerId = entity.Id,
                WebsiteId = w.Id,
                MediaTier = w.MediaTier  // default from website — editable later
            }).ToList();

            if (categories.Any())
                await _categoryRepo.AddRangeAsync(categories);

            // Auto-generate PublicationCustomerCategory rows
            var publications = await _publicationRepo.GetActivePublicationsAsync();
            var pubCategories = publications.Select(p => new PublicationCustomerCategory
            {
                CustomerId = entity.Id,
                PublicationId = p.Id,
                MediaTier = p.MediaTier  // default from publication
            }).ToList();

            if (pubCategories.Any())
                await _publicationCategoryRepo.AddRangeAsync(pubCategories);

            // Auto-generate ChannelCustomerCategory rows
            var channels = await _channelRepo.GetActiveChannelsAsync(); // use whatever your IChannelRepository exposes
            var chanCategories = channels.Select(c => new ChannelCustomerCategory
            {
                CustomerId = entity.Id,
                ChannelId = c.Id,
                MediaTier = c.MediaTier  // default from channel
            }).ToList();

            if (chanCategories.Any())
                await _channelCategoryRepo.AddRangeAsync(chanCategories);

            return (true, "Client created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(ClientDTO model)
        {
            var existing = await _clientRepo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Client not found.");

            if (await _clientRepo.EmailExistsAsync(model.Email, model.Id))
                return (false, "Another client with this email already exists.");

            if (await _clientRepo.UsernameExistsAsync(model.Username, model.Id))
                return (false, "Another client with this username already exists.");

            if (existing.ApplicationUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(existing.ApplicationUserId.Value.ToString());
                if (user != null)
                {
                    user.FullName = model.Name;
                    user.Email = model.Email;
                    user.UserName = model.Username;

                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.Password);
                    }

                    await _userManager.UpdateAsync(user);
                }
            }

            existing.Name = model.Name;
            existing.Email = model.Email;
            existing.Username = model.Username;
            existing.Photo = model.Photo;
            existing.Url = model.Url;
            existing.Contact = model.Contact;
            existing.Notes = model.Notes;
            existing.Status = model.Status;
            existing.Website = model.Website;
            existing.ManagersLimitedNewsDays = model.ManagersLimitedNewsDays;
            existing.UpdatedAt = DateTime.UtcNow; // BUG 4: was existing.UpdatedAt — field is UpdatedDate per BaseEntity

            await _clientRepo.UpdateAsync(existing);
            return (true, "Client updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _clientRepo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Client not found.");

            if (existing.ApplicationUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(existing.ApplicationUserId.Value.ToString());
                if (user != null)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }
            }

            existing.Deleted = 1;
            existing.IsActive = true; // BUG 5: was existing.IsActive — BaseEntity uses Active not IsActive
            existing.DeletedAt = DateTime.UtcNow; // BUG 5: was existing.DeletedAt — field is DeletedDate per BaseEntity

            await _clientRepo.UpdateAsync(existing);
            return (true, "Client deleted successfully.");
        }

        // ── Website Categories ─────────────────────────────────────────────────

        public async Task<IEnumerable<WebsiteCustomerCategoryDTO>> GetClientCategoriesAsync(int clientId)
        {
            var categories = await _categoryRepo.GetByClientIdAsync(clientId);
            return categories.Select(MapCategoryToDTO);
        }

        public async Task<(bool Success, string Message)> UpdateClientCategoriesAsync(UpdateClientCategoriesDTO model)
        {
            var existing = await _categoryRepo.GetByClientIdAsync(model.CustomerId);
            var existingDict = existing.ToDictionary(c => c.Id);
            var toUpdate = new List<WebsiteCustomerCategory>();

            foreach (var dto in model.Categories)
            {
                if (!existingDict.TryGetValue(dto.Id, out var cat)) continue;
                cat.MediaTier = dto.MediaTier;
                cat.Frequency = dto.Frequency;
                cat.Reach = dto.Reach;
                cat.Distribution = dto.Distribution;
                cat.Language = dto.Language;
                cat.UnitPrice = dto.UnitPrice;
                toUpdate.Add(cat);
            }

            if (toUpdate.Any()) await _categoryRepo.UpdateRangeAsync(toUpdate);
            return (true, "Website categories updated successfully.");
        }


        public async Task<IEnumerable<PublicationCustomerCategoryDTO>> GetClientPublicationCategoriesAsync(int clientId)
        {
            var categories = await _publicationCategoryRepo.GetByClientIdAsync(clientId);
            return categories.Select(MapPublicationCategoryToDTO);
        }

        public async Task<(bool Success, string Message)> UpdateClientPublicationCategoriesAsync(UpdateClientPublicationCategoriesDTO model)
        {
            var existing = await _publicationCategoryRepo.GetByClientIdAsync(model.CustomerId);
            var existingDict = existing.ToDictionary(c => c.Id);
            var toUpdate = new List<PublicationCustomerCategory>();

            foreach (var dto in model.Categories)
            {
                if (!existingDict.TryGetValue(dto.Id, out var cat)) continue;
                cat.MediaType = dto.MediaType;
                cat.MediaTier = dto.MediaTier;
                cat.Frequency = dto.Frequency;
                cat.Distribution = dto.Distribution;
                cat.Language = dto.Language;
                cat.UnitPrice = dto.UnitPrice;
                cat.Circulation = dto.Circulation;
                toUpdate.Add(cat);
            }

            if (toUpdate.Any()) await _publicationCategoryRepo.UpdateRangeAsync(toUpdate);
            return (true, "Publication categories updated successfully.");
        }


        public async Task<IEnumerable<ChannelCustomerCategoryDTO>> GetClientChannelCategoriesAsync(int clientId)
        {
            var categories = await _channelCategoryRepo.GetByClientIdAsync(clientId);
            return categories.Select(MapChannelCategoryToDTO);
        }

        public async Task<(bool Success, string Message)> UpdateClientChannelCategoriesAsync(UpdateClientChannelCategoriesDTO model)
        {
            var existing = await _channelCategoryRepo.GetByClientIdAsync(model.CustomerId);
            var existingDict = existing.ToDictionary(c => c.Id);
            var toUpdate = new List<ChannelCustomerCategory>();

            foreach (var dto in model.Categories)
            {
                if (!existingDict.TryGetValue(dto.Id, out var cat)) continue;
                cat.MediaTier = dto.MediaTier;
                cat.Reach = dto.Reach;
                cat.Distribution = dto.Distribution;
                cat.Language = dto.Language;
                cat.UnitPrice = dto.UnitPrice;
                cat.UnitCurrency = dto.UnitCurrency;
                toUpdate.Add(cat);
            }

            if (toUpdate.Any()) await _channelCategoryRepo.UpdateRangeAsync(toUpdate);
            return (true, "Channel categories updated successfully.");
        }


        // ── Assistant CRUD ─────────────────────────────────────────────────────
        // BUG 1: All methods below were missing — now implemented here in ClientService
        // so ClientController only needs to inject IClientService (not a separate IAssistantService)

        public async Task<IEnumerable<AssistantDTO>> GetAssistantsByClientAsync(int clientId)
        {
            var list = await _assistantRepo.GetByClientIdAsync(clientId);
            return list.Select(MapAssistantToDTO);
        }

        public async Task<AssistantDTO?> GetAssistantByIdAsync(int id)
        {
            var a = await _assistantRepo.GetByIdAsync(id);
            return a == null ? null : MapAssistantToDTO(a);
        }

        public async Task<(bool Success, string Message)> CreateAssistantAsync(AssistantDTO model)
        {
            if (await _assistantRepo.EmailExistsAsync(model.Email))
                return (false, "An assistant with this email already exists.");

            if (await _assistantRepo.UsernameExistsAsync(model.Username))
                return (false, "An assistant with this username already exists.");

            if (string.IsNullOrWhiteSpace(model.Password))
                return (false, "Password is required when creating an assistant.");

            var user = new ApplicationUser
            {
                FullName = model.Name,
                UserName = model.Username,
                Email = model.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var userResult = await _userManager.CreateAsync(user, model.Password);
            if (!userResult.Succeeded)
            {
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                return (false, errors);
            }

            await _userManager.AddToRoleAsync(user, "Assistant");

            var entity = new Assistant
            {
                ClientId = model.ClientId,
                Name = model.Name,
                Email = model.Email,
                Username = model.Username,
                Photo = model.Photo,
                Status = model.Status,
                ApplicationUserId = user.Id,
                IsActive = true, // BUG 6: was missing — caused NullRef when GetActiveClientsAsync filtered it out
                Deleted = 0,    // BUG 6: was missing
                CreatedAt = DateTime.UtcNow // BUG 6: was CreatedAt — field is CreatedDate per BaseEntity
            };

            await _assistantRepo.AddAsync(entity);
            return (true, "Assistant created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAssistantAsync(AssistantDTO model)
        {
            var existing = await _assistantRepo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Assistant not found.");

            if (await _assistantRepo.EmailExistsAsync(model.Email, model.Id))
                return (false, "Another assistant with this email already exists.");

            if (await _assistantRepo.UsernameExistsAsync(model.Username, model.Id))
                return (false, "Another assistant with this username already exists.");

            if (existing.ApplicationUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(existing.ApplicationUserId.Value.ToString());
                if (user != null)
                {
                    user.FullName = model.Name;
                    user.Email = model.Email;
                    user.UserName = model.Username;

                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.Password);
                    }

                    await _userManager.UpdateAsync(user);
                }
            }

            existing.Name = model.Name;
            existing.Email = model.Email;
            existing.Username = model.Username;
            existing.Photo = model.Photo;
            existing.Status = model.Status;
            existing.UpdatedAt = DateTime.UtcNow; // BUG 7: was existing.UpdatedAt — field is UpdatedDate per BaseEntity

            await _assistantRepo.UpdateAsync(existing);
            return (true, "Assistant updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAssistantAsync(int id)
        {
            var existing = await _assistantRepo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Assistant not found.");

            if (existing.ApplicationUserId.HasValue)
            {
                var user = await _userManager.FindByIdAsync(existing.ApplicationUserId.Value.ToString());
                if (user != null)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }
            }

            existing.Deleted = 1;
            existing.IsActive = false; // BUG 8: was existing.IsActive — BaseEntity uses Active not IsActive
            existing.DeletedAt = DateTime.UtcNow; // BUG 8: was existing.DeletedAt — field is DeletedDate per BaseEntity

            await _assistantRepo.UpdateAsync(existing);
            return (true, "Assistant deleted successfully.");
        }

        // ── Photo ──────────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> ChangePhotoAsync(int clientId, UploadFileDTO photo)
        {
            var existing = await _clientRepo.GetByIdAsync(clientId);
            if (existing == null)
                return (false, "Client not found.");

            var (savedPath, error) = await SavePhotoAsync(photo);
            if (savedPath == null)
                return (false, error!);

            // Delete old photo file from disk if it exists
            if (!string.IsNullOrEmpty(existing.Photo))
            {
                var oldFullPath = Path.Combine(_env.WebRootPath, existing.Photo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldFullPath))
                    System.IO.File.Delete(oldFullPath);
            }

            existing.Photo = savedPath;
            existing.UpdatedAt = DateTime.UtcNow;

            await _clientRepo.UpdateAsync(existing);
            return (true, "Photo updated successfully.");
        }

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

        private async Task<(string? Path, string? Error)> SavePhotoAsync(UploadFileDTO photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                    return (null, "Please select a photo.");

                if (photo.Length > MaxFileSizeBytes)
                    return (null, "Photo must be smaller than 2 MB.");

                var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                    return (null, "Only JPG, PNG, GIF, and WebP files are allowed.");

                if (!AllowedContentTypes.Contains(photo.ContentType.ToLowerInvariant()))
                    return (null, "Invalid file type.");

                // BUG FIXED: was Directory.GetCurrentDirectory() + "wwwroot" — unreliable in IIS/Azure
                // Must use IWebHostEnvironment.WebRootPath which always resolves correctly
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "clients");
                Directory.CreateDirectory(uploadsFolder); // creates folder if it doesn't exist

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    await photo.FileStream.CopyToAsync(fileStream);

                // Return web-accessible relative path
                return ($"/uploads/clients/{uniqueFileName}", null);
            }
            catch (Exception ex)
            {
                // BUG FIXED: was no try/catch — any IO error crashed the entire request
                return (null, $"Failed to save photo: {ex.Message}");
            }
        }
        public async Task<(bool Success, string Message)> ChangeAssistantPhotoAsync(int assistantid, UploadFileDTO photo)
        {
            var existing = await _assistantRepo.GetByIdAsync(assistantid);
            if (existing == null)
                return (false, "Assistant not found.");

            var (savedPath, error) = await SaveAssistantPhotoAsync(photo);
            if (savedPath == null)
                return (false, error!);

            // Delete old photo file from disk if it exists
            if (!string.IsNullOrEmpty(existing.Photo))
            {
                var oldFullPath = Path.Combine(_env.WebRootPath, existing.Photo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldFullPath))
                    System.IO.File.Delete(oldFullPath);
            }

            existing.Photo = savedPath;
            existing.UpdatedAt = DateTime.UtcNow;

            await _assistantRepo.UpdateAsync(existing);
            return (true, "Photo updated successfully.");
        }

        private async Task<(string? Path, string? Error)> SaveAssistantPhotoAsync(UploadFileDTO photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                    return (null, "Please select a photo.");

                if (photo.Length > MaxFileSizeBytes)
                    return (null, "Photo must be smaller than 2 MB.");

                var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                    return (null, "Only JPG, PNG, GIF, and WebP files are allowed.");

                if (!AllowedContentTypes.Contains(photo.ContentType.ToLowerInvariant()))
                    return (null, "Invalid file type.");

                // BUG FIXED: was Directory.GetCurrentDirectory() + "wwwroot" — unreliable in IIS/Azure
                // Must use IWebHostEnvironment.WebRootPath which always resolves correctly
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "assistants");
                Directory.CreateDirectory(uploadsFolder); // creates folder if it doesn't exist

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    await photo.FileStream.CopyToAsync(fileStream);

                // Return web-accessible relative path
                return ($"/uploads/assistants/{uniqueFileName}", null);
            }
            catch (Exception ex)
            {
                // BUG FIXED: was no try/catch — any IO error crashed the entire request
                return (null, $"Failed to save photo: {ex.Message}");
            }
        }

        // ── Website Categories Excel ──────────────────────────────────────────────────

        public byte[] ExportWebsiteCategoriesToExcel(int clientId, IEnumerable<WebsiteCustomerCategoryDTO> categories)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("WebsiteCategories");

            // Col layout: A=Id  B=WebsiteId  C=WebsiteName  D=WebsiteURL  E=MediaTier  F=UnitPrice
            var headers = new[] { "Id", "WebsiteId", "Website Name", "Website URL", "Media Tier", "Unit Price" };
            WriteHeaders(ws, headers, "#2E75B6");

            int row = 2;
            foreach (var c in categories)
            {
                ws.Cell(row, 1).Value = c.Id;
                ws.Cell(row, 2).Value = c.WebsiteId;
                ws.Cell(row, 3).Value = c.WebsiteName;
                ws.Cell(row, 4).Value = c.WebsiteURL;
                ws.Cell(row, 5).Value = c.MediaTier ?? "";
                ws.Cell(row, 6).Value = c.UnitPrice;
                ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

                // Lock Id, WebsiteId, Name, URL — visually distinguish read-only cols
                ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.FromHtml("#F2F2F2");

                if (row % 2 == 0)
                {
                    ws.Cell(row, 5).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#EBF3FB");
                    ws.Cell(row, 6).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#EBF3FB");
                }

                ws.Range(row, 1, row, 6).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            // Protect editable columns note in A1 comment
            ws.Cell(1, 5).GetComment().AddText("Edit columns E (Media Tier) and F (Unit Price) only. Do not change columns A–D.");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<(bool Success, string Message, int UpdatedCount)>
         ImportWebsiteCategoriesFromExcelAsync(int clientId, Stream fileStream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return (false, "Only .xlsx and .xls files are supported.", 0);

            var existing = (await _categoryRepo.GetByClientIdAsync(clientId)).ToDictionary(c => c.Id);
            if (!existing.Any()) return (false, "No website categories found for this client.", 0);

            var toUpdate = new List<WebsiteCustomerCategory>();
            var errors = new List<string>();

            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook(fileStream);
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
                if (rows == null || rows.Count == 0) return (false, "The file has no data rows.", 0);

                int rowNum = 2;
                foreach (var row in rows)
                {
                    // Col A = Id (key), E=MediaTier, F=Frequency, G=Reach,
                    // H=Distribution, I=Language, J=UnitPrice
                    if (!int.TryParse(row.Cell(1).GetString().Trim(), out int id))
                    {
                        errors.Add($"Row {rowNum}: Id is missing or invalid."); rowNum++; continue;
                    }
                    if (!existing.TryGetValue(id, out var cat))
                    {
                        errors.Add($"Row {rowNum}: Id {id} does not belong to this client."); rowNum++; continue;
                    }

                    decimal.TryParse(row.Cell(10).GetString().Trim(), out decimal price);

                    cat.MediaTier = row.Cell(5).GetString().Trim().NullIfEmpty();
                    cat.Frequency = row.Cell(6).GetString().Trim().NullIfEmpty();
                    cat.Reach = row.Cell(7).GetString().Trim().NullIfEmpty();
                    cat.Distribution = row.Cell(8).GetString().Trim().NullIfEmpty();
                    cat.Language = row.Cell(9).GetString().Trim().NullIfEmpty();
                    cat.UnitPrice = price;
                    toUpdate.Add(cat);
                    rowNum++;
                }
            }
            catch (Exception ex) { return (false, $"Failed to read file: {ex.Message}", 0); }

            if (errors.Any()) return (false, string.Join(" | ", errors), 0);
            if (toUpdate.Any()) await _categoryRepo.UpdateRangeAsync(toUpdate);
            return (true, $"{toUpdate.Count} website category row(s) updated.", toUpdate.Count);
        }


        // ── Publication Categories Excel ──────────────────────────────────────────────
        public byte[] ExportPublicationCategoriesToExcel(
     IEnumerable<PublicationCustomerCategoryDTO> categories, string clientName)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("PublicationCategories");

            // Col layout (10 columns):
            // A=Id  B=PublicationId  C=Publication Name         ← key cols (read-only)
            // D=Media Type  E=Media Tier  F=Frequency
            // G=Distribution  H=Language  I=Unit Price  J=Circulation  ← editable
            var headers = new[]
            {
        "Id", "PublicationId", "Publication Name",
        "Media Type", "Media Tier", "Frequency",
        "Distribution", "Language", "Unit Price", "Circulation"
    };
            WriteHeaders(ws, headers, "#2E75B6");

            int row = 2;
            foreach (var p in categories)
            {
                ws.Cell(row, 1).Value = p.Id;
                ws.Cell(row, 2).Value = p.PublicationId;
                ws.Cell(row, 3).Value = p.PublicationName;
                ws.Cell(row, 4).Value = p.MediaType ?? "";
                ws.Cell(row, 5).Value = p.MediaTier ?? "";
                ws.Cell(row, 6).Value = p.Frequency ?? "";
                ws.Cell(row, 7).Value = p.Distribution ?? "";
                ws.Cell(row, 8).Value = p.Language ?? "";
                ws.Cell(row, 9).Value = p.UnitPrice;
                ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";

                // Circulation: use Blank.Value for null — avoids XLCellValue cast crash
                if (p.Circulation.HasValue)
                    ws.Cell(row, 10).Value = p.Circulation.Value;
                else
                    ws.Cell(row, 10).Value = ClosedXML.Excel.Blank.Value;

                // Gray out read-only key columns A–C
                ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.FromHtml("#F2F2F2");

                // Alternate shading on editable columns D–J
                if (row % 2 == 0)
                    ws.Range(row, 4, row, 10).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.FromHtml("#EBF3FB");

                ws.Range(row, 1, row, 10).Style.Border.OutsideBorder =
                    ClosedXML.Excel.XLBorderStyleValues.Thin;

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Cell(1, 4).GetComment().AddText(
                "KEY COLUMNS (A–C): Do NOT change — used as update keys.\n" +
                "EDITABLE COLUMNS (D–J): Media Type, Media Tier, Frequency, " +
                "Distribution, Language, Unit Price, Circulation.");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }




        // ── Channel Categories Excel ──────────────────────────────────────────────────
        public byte[] ExportChannelCategoriesToExcel(IEnumerable<ChannelCustomerCategoryDTO> categories, string clientName)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("ChannelCategories");

            // Cols: A=Id  B=ChannelId  C=ChannelName
            //       D=MediaTier  E=Reach  F=Distribution  G=Language
            //       H=UnitPrice  I=UnitCurrency
            var headers = new[]
            {
        "Id","ChannelId","Channel Name",
        "Media Tier","Reach","Distribution","Language",
        "Unit Price","Unit Currency"
    };
            WriteHeaders(ws, headers, "#1F4E79");

            int row = 2;
            foreach (var c in categories)
            {
                ws.Cell(row, 1).Value = c.Id;
                ws.Cell(row, 2).Value = c.ChannelId;
                ws.Cell(row, 3).Value = c.ChannelName;
                ws.Cell(row, 4).Value = c.MediaTier ?? "";
                ws.Cell(row, 5).Value = c.Reach ;
                ws.Cell(row, 6).Value = c.Distribution ?? "";
                ws.Cell(row, 7).Value = c.Language ?? "";
                ws.Cell(row, 8).Value = c.UnitPrice;
                ws.Cell(row, 9).Value = c.UnitCurrency;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";

                ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.FromHtml("#F2F2F2");
                if (row % 2 == 0)
                    ws.Range(row, 4, row, 9).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.FromHtml("#D6E4F0");
                ws.Range(row, 1, row, 9).Style.Border.OutsideBorder =
                    ClosedXML.Excel.XLBorderStyleValues.Thin;
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Cell(1, 4).GetComment().AddText(
                "KEY COLUMNS (A–C): Do NOT change — used as update keys.\n" +
                "EDITABLE COLUMNS (D–I): Media Tier, Reach, Distribution, Language, Unit Price, Unit Currency.");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<(bool Success, string Message, int UpdatedCount)> ImportChannelCategoriesFromExcelAsync(
            int clientId, Stream fileStream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return (false, "Only .xlsx and .xls files are supported.", 0);

            var existing = (await _channelCategoryRepo.GetByClientIdAsync(clientId)).ToDictionary(c => c.Id);
            if (!existing.Any())
                return (false, "No channel categories found for this client.", 0);

            var toUpdate = new List<ChannelCustomerCategory>();
            var errors = new List<string>();

            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook(fileStream);
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();

                if (rows == null || rows.Count == 0)
                    return (false, "The file has no data rows.", 0);

                int rowNumber = 2;
                foreach (var row in rows)
                {
                    if (!int.TryParse(row.Cell(1).GetString().Trim(), out int id))
                    {
                        errors.Add($"Row {rowNumber}: Id is missing or invalid.");
                        rowNumber++;
                        continue;
                    }

                    if (!existing.TryGetValue(id, out var entity))
                    {
                        errors.Add($"Row {rowNumber}: Id {id} does not belong to this client.");
                        rowNumber++;
                        continue;
                    }

                    decimal.TryParse(row.Cell(5).GetString().Trim(), out decimal unitPrice);

                    entity.MediaTier = row.Cell(4).GetString().Trim().NullIfEmpty();
                    entity.UnitPrice = unitPrice;
                    toUpdate.Add(entity);

                    rowNumber++;
                }
            }
            catch (Exception ex)
            {
                return (false, $"Failed to read file: {ex.Message}", 0);
            }

            if (errors.Any())
                return (false, string.Join(" | ", errors), 0);

            if (toUpdate.Any())
                await _channelCategoryRepo.UpdateRangeAsync(toUpdate);

            return (true, $"{toUpdate.Count} channel category row(s) updated.", toUpdate.Count);
        }

        // ── Mappers ────────────────────────────────────────────────────────────

        private static ClientDTO MapToDTO(EditorEntitiesLayer.Entities.Client c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Username = c.Username,
            Photo = c.Photo,
            Url = c.Url,
            Contact = c.Contact,
            Notes = c.Notes,
            Status = c.Status,
            Website = c.Website,
            ManagersLimitedNewsDays = c.ManagersLimitedNewsDays,
            ApplicationUserId = c.ApplicationUserId
        };

        private static EditorEntitiesLayer.Entities.Client MapToEntity(ClientDTO dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Username = dto.Email,
            Photo = dto.Photo,
            Url = dto.Url,
            Contact = dto.Contact,
            Notes = dto.Notes,
            Status = dto.Status,
            Website = dto.Website,
            ManagersLimitedNewsDays = dto.ManagersLimitedNewsDays,
            IsActive = true, // BUG 9: was IsActive — BaseEntity uses Active not IsActive
            Deleted = 0,

        };

        private static AssistantDTO MapAssistantToDTO(Assistant a) => new()
        {
            Id = a.Id,
            ClientId = a.ClientId,
            ClientName = a.Client?.Name,
            Name = a.Name,
            Email = a.Email,
            Username = a.Username,
            Photo = a.Photo,
            Status = a.Status,
            ApplicationUserId = a.ApplicationUserId
        };

        private static WebsiteCustomerCategoryDTO MapCategoryToDTO(WebsiteCustomerCategory c) => new()
        {
            Id = c.Id,
            CustomerId = c.CustomerId,
            WebsiteId = c.WebsiteId,
            WebsiteName = c.Website?.WebsiteName ?? string.Empty,
            WebsiteURL = c.Website?.URL ?? string.Empty,
            MediaTier = c.MediaTier,
            Frequency = c.Frequency,
            Reach = c.Reach,
            Distribution = c.Distribution,
            Language = c.Language,
            UnitPrice = c.UnitPrice
        };

        private static PublicationCustomerCategoryDTO MapPublicationCategoryToDTO(
    PublicationCustomerCategory p) => new()
    {
        Id = p.Id,
        CustomerId = p.CustomerId,
        PublicationId = p.PublicationId,
        PublicationName = p.Publication?.PublicationName ?? string.Empty,
        // PublicationURL not mapped — Publication entity has no URL field
        MediaType = p.MediaType,
        MediaTier = p.MediaTier,
        Frequency = p.Frequency,
        Distribution = p.Distribution,
        Language = p.Language,
        UnitPrice = p.UnitPrice,
        Circulation = p.Circulation
    };

        private static ChannelCustomerCategoryDTO MapChannelCategoryToDTO(ChannelCustomerCategory c) => new()
        {
            Id = c.Id,
            CustomerId = c.CustomerId,
            ChannelId = c.ChannelId,
            ChannelName = c.Channel?.ChannelName ?? string.Empty,
            MediaTier = c.MediaTier,
            Reach = c.Reach,
            Distribution = c.Distribution,
            Language = c.Language,
            UnitPrice = c.UnitPrice,
            UnitCurrency = c.UnitCurrency
        };


        // ── Website Categories Export / Import ────────────────────────────────────────
        public byte[] ExportWebsiteCategoriesToExcel(IEnumerable<WebsiteCustomerCategoryDTO> categories, string clientName)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("WebsiteCategories");

            // Cols: A=Id  B=WebsiteId  C=WebsiteName  D=WebsiteURL
            //       E=MediaTier  F=Frequency  G=Reach  H=Distribution  I=Language  J=UnitPrice
            var headers = new[]
            {
        "Id","WebsiteId","Website Name","Website URL",
        "Media Tier","Frequency","Reach","Distribution","Language","Unit Price"
    };
            WriteHeaders(ws, headers, "#2E75B6");

            int row = 2;
            foreach (var c in categories)
            {
                ws.Cell(row, 1).Value = c.Id;
                ws.Cell(row, 2).Value = c.WebsiteId;
                ws.Cell(row, 3).Value = c.WebsiteName;
                ws.Cell(row, 4).Value = c.WebsiteURL;
                ws.Cell(row, 5).Value = c.MediaTier ?? "";
                ws.Cell(row, 6).Value = c.Frequency ?? "";
                ws.Cell(row, 7).Value = c.Reach ?? "";
                ws.Cell(row, 8).Value = c.Distribution ?? "";
                ws.Cell(row, 9).Value = c.Language ?? "";
                ws.Cell(row, 10).Value = c.UnitPrice;
                ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";

                // Gray out read-only key columns A–D
                ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.FromHtml("#F2F2F2");
                if (row % 2 == 0)
                    ws.Range(row, 5, row, 10).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.FromHtml("#EBF3FB");
                ws.Range(row, 1, row, 10).Style.Border.OutsideBorder =
                    ClosedXML.Excel.XLBorderStyleValues.Thin;
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            ws.Cell(1, 5).GetComment().AddText(
                "KEY COLUMNS (A–D): Do NOT change — used as update keys.\n" +
                "EDITABLE COLUMNS (E–J): Media Tier, Frequency, Reach, Distribution, Language, Unit Price.");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ── Publication Categories Export / Import ────────────────────────────────────
        public async Task<(bool Success, string Message, int UpdatedCount)>
     ImportPublicationCategoriesFromExcelAsync(
         int clientId, Stream fileStream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return (false, "Only .xlsx and .xls files are supported.", 0);

            var existing = (await _publicationCategoryRepo.GetByClientIdAsync(clientId))
                               .ToDictionary(c => c.Id);
            if (!existing.Any())
                return (false, "No publication categories found for this client.", 0);

            var toUpdate = new List<PublicationCustomerCategory>();
            var errors = new List<string>();

            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook(fileStream);
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
                if (rows == null || rows.Count == 0)
                    return (false, "The file has no data rows.", 0);

                int rowNum = 2;
                foreach (var row in rows)
                {
                    // Col A=Id (key — do not change)
                    // Col D=MediaType, E=MediaTier, F=Frequency,
                    // Col G=Distribution, H=Language, I=UnitPrice, J=Circulation
                    if (!int.TryParse(row.Cell(1).GetString().Trim(), out int id))
                    {
                        errors.Add($"Row {rowNum}: Id is missing or invalid.");
                        rowNum++; continue;
                    }
                    if (!existing.TryGetValue(id, out var cat))
                    {
                        errors.Add($"Row {rowNum}: Id {id} does not belong to this client.");
                        rowNum++; continue;
                    }

                    decimal.TryParse(row.Cell(9).GetString().Trim(), out decimal price);
                    int.TryParse(
                        row.Cell(10).GetString().Trim().Replace(",", ""),
                        out int circulation);

                    cat.MediaType = row.Cell(4).GetString().Trim().NullIfEmpty();
                    cat.MediaTier = row.Cell(5).GetString().Trim().NullIfEmpty();
                    cat.Frequency = row.Cell(6).GetString().Trim().NullIfEmpty();
                    cat.Distribution = row.Cell(7).GetString().Trim().NullIfEmpty();
                    cat.Language = row.Cell(8).GetString().Trim().NullIfEmpty();
                    cat.UnitPrice = price;
                    cat.Circulation = circulation > 0 ? circulation : null;
                    toUpdate.Add(cat);
                    rowNum++;
                }
            }
            catch (Exception ex)
            {
                return (false, $"Failed to read file: {ex.Message}", 0);
            }

            if (errors.Any())
                return (false, string.Join(" | ", errors), 0);

            if (toUpdate.Any())
                await _publicationCategoryRepo.UpdateRangeAsync(toUpdate);

            return (true, $"{toUpdate.Count} publication category row(s) updated.", toUpdate.Count);
        }

        // ── Shared Excel header helper ────────────────────────────────────────────────
        private static void WriteHeaders(ClosedXML.Excel.IXLWorksheet ws, string[] headers, string hexColor)
        {
            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cell(1, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml(hexColor);
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.White;
            }
        }


      
    }
}
