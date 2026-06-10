using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorRepositoryLayer.Repositories;
using EditorViewModelLayer.MediaViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.ClientVideoLogic
{
    public class ClientVideoService : IClientVideoService
    {
        private readonly IClientVideoRepository _clientVideoRepo;
        private readonly IGeneralVideosRepository _generalVideosRepo;
        private readonly IChannelRepository _channelRepo;
        private readonly IClientRepository _clientRepo;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ClientVideoService(
            IClientVideoRepository clientVideoRepo,
            IGeneralVideosRepository generalVideosRepo,
            IChannelRepository channelRepo,
            IClientRepository clientRepo,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _clientVideoRepo = clientVideoRepo;
            _generalVideosRepo = generalVideosRepo;
            _channelRepo = channelRepo;
            _clientRepo = clientRepo;
            _context = context;
            _env = env;
        }

        // ── List ───────────────────────────────────────────────────────────────


        public async Task<ClientVideoListDTO> GetListAsync(int clientId)
        {
            var client = await _clientRepo.GetByIdAsync(clientId);
            var rawItems = (await _clientVideoRepo.GetByClientIdAsync(clientId)).ToList();

            if (!rawItems.Any())
                return new ClientVideoListDTO { ClientId = clientId, ClientName = client?.Name ?? string.Empty };

            var categoryIds = rawItems.Select(a => a.CategoryId).Distinct().ToList();
            var subCategoryIds = rawItems.Select(a => a.SubCategoryId).Distinct().ToList();
            var channelIds = rawItems.Select(a => a.ChannelId).Distinct().ToList();
            var userIds = rawItems.Select(a => a.CreateId).Distinct().ToList();

            var categoryMap = await _context.Set<ClientCategories>()
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CategoryName);

            var subCategoryMap = await _context.Set<ClientCategories>()
                .Where(c => subCategoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CategoryName);

            var channelMap = await _context.Set<EditorEntitiesLayer.Entities.Channel>()
                .Where(c => channelIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.ChannelName);

            var userMap = await _context.Set<ApplicationUser>()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName ?? string.Empty);

            var dtos = rawItems
                .Select(e => MapToDTOWithLookups(e, categoryMap, subCategoryMap, channelMap, userMap))
                .ToList();

            return new ClientVideoListDTO
            {
                ClientId = clientId,
                ClientName = client?.Name ?? string.Empty,
                Items = dtos
            };
        }


        public async Task<ClientVideoDTO?> GetByIdAsync(int id)
        {
            var e = await _clientVideoRepo.GetByIdWithDetailsAsync(id);
            if (e == null) return null;

            var dto = MapToDTO(e);

            if (e.ChannelId > 0)
            {
                var ch = await _context.Set<EditorEntitiesLayer.Entities.Channel>().FindAsync(e.ChannelId);
                dto.ChannelName = ch?.ChannelName;
            }
            if (e.CategoryId > 0)
            {
                var cat = await _context.Set<ClientCategories>().FindAsync(e.CategoryId);
                dto.CategoryName = cat?.CategoryName;
            }
            if (e.SubCategoryId > 0)
            {
                var sub = await _context.Set<ClientCategories>().FindAsync(e.SubCategoryId);
                dto.SubCategoryName = sub?.CategoryName;
            }
            if (e.CreateId > 0)
            {
                var user = await _context.Set<ApplicationUser>().FindAsync(e.CreateId);
                dto.CreatedByUserName = user?.FullName ?? user?.UserName;
            }
            dto.CreatedAt = e.CreatedAt;

            return dto;
        }


        // ── Create ─────────────────────────────────────────────────────────────
        // Step 1: Insert GeneralVideos master
        // Step 2: Insert ClientVideo with real VideoId

        public async Task<(bool Success, string Message)> CreateAsync(
            ClientVideoDTO model, IFormFileCollection files)
        {
            // Auto-calculate
            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = model.Impression * 4;

            // Save uploaded files
            var (videoFile, screenshot) = await SaveVideoFilesAsync(files);

            // Step 1 — GeneralVideos master
            var general = new GeneralVideos
            {
                Date = model.Date,
                Title = model.Title,
                ChannelId = model.ChannelId,
                description = null,
                VideoUrl = model.VideoUrl ?? string.Empty,
                VideoFileFile = videoFile ?? string.Empty,
                ScreenshotFile = screenshot ?? string.Empty,
                Duration = model.Duration,
                Toning = model.Toning,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _generalVideosRepo.AddAsync(general); // general.Id set by EF

            // Step 2 — ClientVideo with real VideoId
            var clientVideo = new ClientVideo
            {
                VideoId = general.Id,
                ClientId = model.ClientId,
                Date = model.Date,
                Title = model.Title,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                description = model.Program,
                ChannelId = model.ChannelId,
                VideoUrl = model.VideoUrl ?? string.Empty,
                VideoFileFile = videoFile ?? string.Empty,
                ScreenshotFile = screenshot ?? string.Empty,
                Duration = model.Duration,
                Toning = model.Toning,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _clientVideoRepo.AddAsync(clientVideo);

            return (true, "Video created successfully.");
        }

        // ── Update ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> UpdateAsync(
            ClientVideoDTO model, IFormFileCollection files)
        {
            var existing = await _clientVideoRepo.GetByIdWithDetailsAsync(model.Id);
            if (existing == null) return (false, "Record not found.");

            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = model.Impression * 4;

            var (videoFile, screenshot) = await SaveVideoFilesAsync(files);

            existing.ChannelId = model.ChannelId;
            existing.CategoryId = model.CategoryId;
            existing.SubCategoryId = model.SubCategoryId;
            existing.Date = model.Date;
            existing.Title = model.Title;
            existing.description = model.Program;
            existing.Duration = model.Duration;
            existing.Toning = model.Toning;
            existing.VideoUrl = model.VideoUrl ?? existing.VideoUrl;
            existing.UpdatedAt = DateTime.UtcNow;
            if (videoFile != null) existing.VideoFileFile = videoFile;
            if (screenshot != null) existing.ScreenshotFile = screenshot;

            // Update GeneralVideos master
            var general = await _generalVideosRepo.GetByIdAsync(existing.VideoId);
            if (general != null)
            {
                general.Date = model.Date;
                general.Title = model.Title;
                general.ChannelId = model.ChannelId;
                general.VideoUrl = model.VideoUrl ?? general.VideoUrl;
                general.Duration = model.Duration;
                general.Toning = model.Toning;
                general.UpdatedAt = DateTime.UtcNow;
                if (videoFile != null) general.VideoFileFile = videoFile;
                if (screenshot != null) general.ScreenshotFile = screenshot;
                await _generalVideosRepo.UpdateAsync(general);
            }

            await _clientVideoRepo.UpdateAsync(existing);
            return (true, "Video updated successfully.");
        }

        // ── Delete (soft) ──────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _clientVideoRepo.GetByIdWithDetailsAsync(id);
            if (existing == null) return (false, "Record not found.");
            existing.IsActive = false;
            existing.Deleted = 1;
            existing.DeletedAt = DateTime.UtcNow;
            await _clientVideoRepo.UpdateAsync(existing);
            return (true, "Record deleted.");
        }

        // ── Dropdown builders ──────────────────────────────────────────────────

        public async Task<List<MediaSelectOption>> GetChannelOptionsAsync(int selectedId = 0)
        {
            var channels = await _channelRepo.GetActiveChannelsAsync();
            return channels.Select(c => new MediaSelectOption
            {
                Value = c.Id.ToString(),
                Text = c.ChannelName,
                Selected = c.Id == selectedId
            }).ToList();
        }

        public async Task<List<MediaSelectOption>> GetCategoryOptionsAsync(int clientId, int selectedId = 0)
        {
            var cats = await _context.Set<ClientCategories>()
                .Where(c => c.ClientId == clientId && c.ParentCategory == null
                         && c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.Order).ThenBy(c => c.CategoryName).ToListAsync();
            return cats.Select(c => new MediaSelectOption
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName,
                Selected = c.Id == selectedId
            }).ToList();
        }

        public async Task<List<MediaSelectOption>> GetSubCategoryOptionsAsync(int parentId, int selectedId = 0)
        {
            if (parentId == 0) return new List<MediaSelectOption>();
            var cats = await _context.Set<ClientCategories>()
                .Where(c => c.ParentCategory == parentId && c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.Order).ThenBy(c => c.CategoryName).ToListAsync();
            return cats.Select(c => new MediaSelectOption
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName,
                Selected = c.Id == selectedId
            }).ToList();
        }

        // ── AJAX auto-fill ─────────────────────────────────────────────────────

        public async Task<ChannelAutoFillDTO> GetChannelAutoFillAsync(int channelId, int clientId)
        {
            var channel = await _channelRepo.GetByIdAsync(channelId);
            if (channel == null) return new ChannelAutoFillDTO();

            var ccc = await _context.Set<ChannelCustomerCategory>()
                .FirstOrDefaultAsync(c => c.ChannelId == channelId && c.CustomerId == clientId);

            var adValue = ccc.UnitPrice;
            var impression = ccc.Reach; // extend Channel entity with Impression if needed

            return new ChannelAutoFillDTO
            {
                AdValue = adValue,
                PrValue = Math.Round(adValue * 3.5m, 2),
                MediaTier = ccc?.MediaTier ?? channel.MediaTier,
                Impression = impression,
                Reach = impression * 4
            };
        }

        // ── File helpers ───────────────────────────────────────────────────────

        private async Task<(string? VideoFile, string? Screenshot)> SaveVideoFilesAsync(
            IFormFileCollection files)
        {
            string? videoFile = null;
            string? screenshot = null;
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "videos");
            Directory.CreateDirectory(uploadDir);

            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                var ext = Path.GetExtension(file.FileName);
                var unique = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadDir, unique);
                using var fs = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(fs);
                var webPath = $"/uploads/videos/{unique}";
                if (file.Name == "videoFile") videoFile = webPath;
                if (file.Name == "screenshotFile") screenshot = webPath;
            }
            return (videoFile, screenshot);
        }

        // ── Mapper ─────────────────────────────────────────────────────────────

        private static ClientVideoDTO MapToDTOWithLookups(
            ClientVideo e,
            Dictionary<int, string> categoryMap,
            Dictionary<int, string> subCategoryMap,
            Dictionary<int, string> channelMap,
            Dictionary<int, string> userMap) => new()
            {
                Id = e.Id,
                VideoId = e.VideoId,
                ClientId = e.ClientId,
                Date = e.Date,
                ChannelId = e.ChannelId,
                ChannelName = channelMap.GetValueOrDefault(e.ChannelId),
                CategoryId = e.CategoryId,
                CategoryName = categoryMap.GetValueOrDefault(e.CategoryId),
                SubCategoryId = e.SubCategoryId,
                SubCategoryName = subCategoryMap.GetValueOrDefault(e.SubCategoryId),
                Title = e.Title ?? string.Empty,
                Program = e.description,
                Duration = (int)e.Duration,
                Toning = e.Toning,
                VideoUrl = e.VideoUrl,
                VideoFile = e.VideoFileFile,
                Screenshot = e.ScreenshotFile,
                ADValue = e.ADValue ?? 0,
                PRValue = e.PRValue ?? 0,
                CreatedAt = e.CreatedAt,
                CreatedByUserName = userMap.GetValueOrDefault(e.CreateId)
            };


        private static ClientVideoDTO MapToDTO(ClientVideo e) => new()
        {
            Id = e.Id,
            VideoId = e.VideoId,
            ClientId = e.ClientId,
            Date = e.Date,
            ChannelId = e.ChannelId,
            CategoryId = e.CategoryId,
            SubCategoryId = e.SubCategoryId,
            Title = e.Title ?? string.Empty,
            Program = e.description,
            Duration = (int)e.Duration,
            Toning = e.Toning,
            VideoUrl = e.VideoUrl,
            VideoFile = e.VideoFileFile,
            Screenshot = e.ScreenshotFile,
            ADValue = e.ADValue ?? 0,
            PRValue = e.PRValue ?? 0,
            CreatedAt = e.CreatedAt
        };
    }

}
