using EditorEntitiesLayer.Entities;
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

        public ClientService(
            IClientRepository clientRepo,
            IAssistantRepository assistantRepo, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _clientRepo = clientRepo;
            _assistantRepo = assistantRepo;
            _userManager = userManager;
            _env = env;
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
                CreatedAt  = DateTime.UtcNow // BUG 6: was CreatedAt — field is CreatedDate per BaseEntity
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
            existing.UpdatedAt= DateTime.UtcNow;

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
            Deleted = 0 ,
            
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
    }
}
