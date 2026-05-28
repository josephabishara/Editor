using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.MediaViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.ClientArticleLogic
{
    public class ClientArticleService : IClientArticleService
    {
        private readonly IClientArticleRepository _clientArticleRepo;
        private readonly IGeneralArticleRepository _generalArticleRepo;
        private readonly IWebsiteRepository _websiteRepo;
        private readonly IWriterRepository _writerRepo;
        private readonly IClientRepository _clientRepo;
        private readonly ApplicationDbContext _context;

        public ClientArticleService(
            IClientArticleRepository clientArticleRepo,
            IGeneralArticleRepository generalArticleRepo,
            IWebsiteRepository websiteRepo,
            IWriterRepository writerRepo,
            IClientRepository clientRepo,
            ApplicationDbContext context)
        {
            _clientArticleRepo = clientArticleRepo;
            _generalArticleRepo = generalArticleRepo;
            _websiteRepo = websiteRepo;
            _writerRepo = writerRepo;
            _clientRepo = clientRepo;
            _context = context;
        }

        // ── List ───────────────────────────────────────────────────────────────

        public async Task<ClientArticleListDTO> GetListAsync(int clientId)
        {
            var client = await _clientRepo.GetByIdAsync(clientId);
            var items = await _clientArticleRepo.GetByClientIdAsync(clientId);
            return new ClientArticleListDTO
            {
                ClientId = clientId,
                ClientName = client?.Name ?? string.Empty,
                Items = items.Select(MapToDTO).ToList()
            };
        }

        public async Task<ClientArticleDTO?> GetByIdAsync(int id)
        {
            var e = await _clientArticleRepo.GetByIdWithDetailsAsync(id);
            return e == null ? null : MapToDTO(e);
        }

        // ── Create ─────────────────────────────────────────────────────────────
        // Step 1: Insert GeneralArticle (no FK dependency)
        // Step 2: Insert ClientArticle with real ArticleId

        public async Task<(bool Success, string Message)> CreateAsync(ClientArticleDTO model)
        {
            // Auto-calculate
            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = model.Impression * 4;

            // Step 1 — GeneralArticle master
 
            var general = new EditorEntitiesLayer.Entities.GeneralArticle
            {
                Date = model.Date,
                WebsiteId = model.WebsiteId,
                WriterId = model.WriterId,
                Language = model.Language,
                ArticleBranding = model.ArticleBranding,
                HeadlineBranding = model.HeadlineBranding,
                PictureinArticle = model.PictureinArticle,
                Generation = model.Generation == "Generated",
                ArticleURL = model.ArticleURL,
                Title = model.Title,
                Content = model.Content,
                Images = model.Images,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _generalArticleRepo.AddAsync(general); // general.Id set by EF

            // Step 2 — ClientArticle with real ArticleId
            var clientArticle = new ClientArticle
            {
                ArticleId = general.Id,
                ClientId = model.ClientId,
                Date = model.Date,
                WebsiteId = model.WebsiteId,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                WriterId = model.WriterId,
                Frequency = model.Frequency,
                MediaType = model.MediaType,
                Impression = model.Impression,
                Reach = model.Reach,
                ADValue = model.ADValue,
                PRValue = model.PRValue,
                Toning = model.Toning,
                MediaTier = model.MediaTier,
                Language = model.Language,
                ArticleBranding = model.ArticleBranding,
                HeadlineBranding = model.HeadlineBranding,
                PictureinArticle = model.PictureinArticle,
                Generation = model.Generation == "Generated",
                ArticleURL = model.ArticleURL,
                Title = model.Title,
                Content = model.Content,
                Images = model.Images,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _clientArticleRepo.AddAsync(clientArticle);

            return (true, "Article created successfully.");
        }

        // ── Update ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> UpdateAsync(ClientArticleDTO model)
        {
            var existing = await _clientArticleRepo.GetByIdWithDetailsAsync(model.Id);
            if (existing == null) return (false, "Record not found.");

            // Recalculate
            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = model.Impression * 4;

            existing.WebsiteId = model.WebsiteId;
            existing.CategoryId = model.CategoryId;
            existing.SubCategoryId = model.SubCategoryId;
            existing.WriterId = model.WriterId;
            existing.Date = model.Date;
            existing.Frequency = model.Frequency;
            existing.MediaType = model.MediaType;
            existing.Impression = model.Impression;
            existing.Reach = model.Reach;
            existing.ADValue = model.ADValue;
            existing.PRValue = model.PRValue;
            existing.Toning = model.Toning;
            existing.MediaTier = model.MediaTier;
            existing.Language = model.Language;
            existing.ArticleBranding = model.ArticleBranding;
            existing.HeadlineBranding = model.HeadlineBranding;
            existing.PictureinArticle = model.PictureinArticle;
            existing.Generation = model.Generation == "Generated";
            existing.ArticleURL = model.ArticleURL;
            existing.Title = model.Title;
            existing.Content = model.Content;
            existing.Images = model.Images;
            existing.UpdatedAt = DateTime.UtcNow;

            // Also update GeneralArticle master
            var general = await _generalArticleRepo.GetByIdAsync(existing.ArticleId);
            if (general != null)
            {
                general.Date = model.Date;
                general.WebsiteId = model.WebsiteId;
                general.WriterId = model.WriterId;
                general.Language = model.Language;
                general.ArticleBranding = model.ArticleBranding;
                general.HeadlineBranding = model.HeadlineBranding;
                general.PictureinArticle = model.PictureinArticle;
                general.Generation = model.Generation == "Generated";
                general.ArticleURL = model.ArticleURL;
                general.Title = model.Title;
                general.Content = model.Content;
                general.Images = model.Images;
                general.UpdatedAt = DateTime.UtcNow;
                await _generalArticleRepo.UpdateAsync(general);
            }

            await _clientArticleRepo.UpdateAsync(existing);
            return (true, "Article updated successfully.");
        }

        // ── Delete (soft) ──────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _clientArticleRepo.GetByIdWithDetailsAsync(id);
            if (existing == null) return (false, "Record not found.");

            existing.IsActive = false;
            existing.Deleted = 1;
            existing.DeletedAt = DateTime.UtcNow;
            await _clientArticleRepo.UpdateAsync(existing);
            return (true, "Record deleted.");
        }

        // ── Dropdown builders ──────────────────────────────────────────────────

        public async Task<List<MediaSelectOption>> GetWebsiteOptionsAsync(int selectedId = 0)
        {
            var sites = await _websiteRepo.GetActiveWebsitesAsync();
            return sites.Select(w => new MediaSelectOption
            {
                Value = w.Id.ToString(),
                Text = w.WebsiteName,
                Selected = w.Id == selectedId
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

        public async Task<List<MediaSelectOption>> GetWriterOptionsAsync(int selectedId = 0)
        {
            var writers = await _writerRepo.GetActiveWritersAsync();
            return writers.Select(w => new MediaSelectOption
            {
                Value = w.Id.ToString(),
                Text = w.WriterName,
                Selected = w.Id == selectedId
            }).ToList();
        }

        // ── AJAX auto-fill ─────────────────────────────────────────────────────

        public async Task<WebsiteAutoFillDTO> GetWebsiteAutoFillAsync(int websiteId, int clientId)
        {
            var site = await _websiteRepo.GetByIdAsync(websiteId);
            if (site == null) return new WebsiteAutoFillDTO();

            var wcc = await _context.Set<WebsiteCustomerCategory>()
                .FirstOrDefaultAsync(w => w.WebsiteId == websiteId && w.CustomerId == clientId);

            var adValue = site.UnitPrice;
            var impression = 0; // Websites don't have Impression field in entity — extend if needed

            return new WebsiteAutoFillDTO
            {
                AdValue = adValue,
                PrValue = Math.Round(adValue * 3.5m, 2),
                MediaType = null,  // Websites entity has no MediaType — extend if needed
                MediaTier = wcc?.MediaTier ?? site.MediaTier,
                Frequency = site.Frequency,
                Language = site.Language,
                Impression = impression,
                Reach = impression * 4
            };
        }

        // ── Mapper ─────────────────────────────────────────────────────────────

        private static ClientArticleDTO MapToDTO(ClientArticle e) => new()
        {
            Id = e.Id,
            ArticleId = e.ArticleId,
            ClientId = e.ClientId,
            Date = e.Date,
            WebsiteId = e.WebsiteId,
            CategoryId = e.CategoryId,
            SubCategoryId = e.SubCategoryId,
            WriterId = e.WriterId,
            Frequency = e.Frequency,
            MediaType = e.MediaType,
            Impression = e.Impression,
            Reach = e.Reach,
            ADValue = e.ADValue ?? 0,
            PRValue = e.PRValue ?? 0,
            Toning = e.Toning,
            MediaTier = e.MediaTier,
            Language = e.Language,
            ArticleBranding = e.ArticleBranding,
            HeadlineBranding = e.HeadlineBranding,
            PictureinArticle = e.PictureinArticle,
            Generation = e.Generation == true ? "Generated" : "Not Generated",
            ArticleURL = e.ArticleURL,
            Title = e.Title ?? string.Empty,
            Content = e.Content,
            Images = e.Images
        };
    }

}
