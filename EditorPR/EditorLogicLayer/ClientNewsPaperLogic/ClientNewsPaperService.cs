using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.MediaViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.ClientNewsPaperLogic
{
    public class ClientNewsPaperService : IClientNewsPaperService
    {
        private readonly IClientNewsPaperRepository _clientNewsPaperRepo;
        private readonly INewsPaperRepository _newsPaperRepo;
        private readonly IPublicationRepository _publicationRepo;
        private readonly IWriterRepository _writerRepo;
        private readonly IClientRepository _clientRepo;
        private readonly ApplicationDbContext _context;

        public ClientNewsPaperService(
            IClientNewsPaperRepository clientNewsPaperRepo,
            INewsPaperRepository newsPaperRepo,
            IPublicationRepository publicationRepo,
            IWriterRepository writerRepo,
            IClientRepository clientRepo,
            ApplicationDbContext context)
        {
            _clientNewsPaperRepo = clientNewsPaperRepo;
            _newsPaperRepo = newsPaperRepo;
            _publicationRepo = publicationRepo;
            _writerRepo = writerRepo;
            _clientRepo = clientRepo;
            _context = context;
        }

        // ── List ───────────────────────────────────────────────────────────────

        public async Task<ClientNewsPaperListDTO> GetListAsync(int clientId)
        {
            var client = await _clientRepo.GetByIdAsync(clientId);
            var items = await _clientNewsPaperRepo.GetByClientIdAsync(clientId);
            return new ClientNewsPaperListDTO
            {
                ClientId = clientId,
                ClientName = client?.Name ?? string.Empty,
                Items = items.Select(MapToDTO).ToList()
            };
        }

        public async Task<ClientNewsPaperDTO?> GetByIdAsync(int id)
        {
            var entity = await _clientNewsPaperRepo.GetByIdWithDetailsAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        // ── Create ─────────────────────────────────────────────────────────────
        // Step 1: Insert NewsPaper master (no FK dependency)
        // Step 2: Insert ClientNewsPaper with real NewsPaperId

        public async Task<(bool Success, string Message)> CreateAsync(ClientNewsPaperDTO model)
        {
            // Auto-calculate before save
            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = (model.Circulation ?? 0) * 4;

            // Step 1 — insert NewsPaper master
            var newsPaper = new NewsPaper
            {
                PublicationId = model.PublicationId,
                Date = model.Date,
                Title = model.Title,
                ADValue = model.ADValue,
                PRValue = model.PRValue,
                ArticleBranding = model.ArticleBranding,
                HeadlineBranding = model.HeadlineBranding,
                Toning = model.Toning,
                Content = model.Content,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _newsPaperRepo.AddAsync(newsPaper); // newsPaper.Id set by EF

            // Step 2 — insert ClientNewsPaper with real NewsPaperId
            var clientNewsPaper = new ClientNewsPaper
            {
                NewsPaperId = newsPaper.Id,       // real Id — no FK violation
                ClientId = model.ClientId,
                PublicationId = model.PublicationId,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                WriterId = model.WriterId,
                Date = model.Date,
                Title = model.Title,
                Pages = model.Pages,
                Height = model.Height,
                Width = model.Width,
                ADValue = model.ADValue,
                PRValue = model.PRValue,
                ArticleBranding = model.ArticleBranding,
                HeadlineBranding = model.HeadlineBranding,
                Toning = model.Toning,
                Content = model.Content,
                //Images = model.Images,
                Publish = false,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _clientNewsPaperRepo.AddAsync(clientNewsPaper);

            return (true, "Newspaper article created successfully.");
        }

        // ── Update ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> UpdateAsync(ClientNewsPaperDTO model)
        {
            var existing = await _clientNewsPaperRepo.GetByIdWithDetailsAsync(model.Id);
            if (existing == null) return (false, "Record not found.");

            // Recalculate
            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = (model.Circulation ?? 0) * 4;

            existing.PublicationId = model.PublicationId;
            existing.CategoryId = model.CategoryId;
            existing.SubCategoryId = model.SubCategoryId;
            existing.WriterId = model.WriterId;
            existing.Date = model.Date;
            existing.Title = model.Title;
            existing.Pages = model.Pages;
            existing.Height = model.Height;
            existing.Width = model.Width;
            existing.ADValue = model.ADValue;
            existing.PRValue = model.PRValue;
            existing.ArticleBranding = model.ArticleBranding;
            existing.HeadlineBranding = model.HeadlineBranding;
            existing.Toning = model.Toning;
            existing.Content = model.Content;
            //existing.Images = model.Images;
            existing.UpdatedAt = DateTime.UtcNow;

            // Also update NewsPaper master
            if (existing.NewsPaper != null)
            {
                existing.NewsPaper.Title = model.Title;
                existing.NewsPaper.ADValue = model.ADValue;
                existing.NewsPaper.PRValue = model.PRValue;
                existing.NewsPaper.ArticleBranding = model.ArticleBranding;
                existing.NewsPaper.HeadlineBranding = model.HeadlineBranding;
                existing.NewsPaper.Toning = model.Toning;
                existing.NewsPaper.Content = model.Content;
                existing.NewsPaper.UpdatedAt = DateTime.UtcNow;
                await _newsPaperRepo.UpdateAsync(existing.NewsPaper);
            }

            await _clientNewsPaperRepo.UpdateAsync(existing);
            return (true, "Newspaper article updated successfully.");
        }

        // ── Delete (soft) ──────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _clientNewsPaperRepo.GetByIdWithDetailsAsync(id);
            if (existing == null) return (false, "Record not found.");

            existing.IsActive = false;
            existing.Deleted = 1;
            existing.DeletedAt = DateTime.UtcNow;
            await _clientNewsPaperRepo.UpdateAsync(existing);

            // Soft-delete master only if no other clients reference it
            var others = await _clientNewsPaperRepo.GetByNewsPaperIdAsync(existing.NewsPaperId);
            if (!others.Any(o => o.Id != id))
            {
                var master = existing.NewsPaper;
                master.IsActive = false;
                master.Deleted = 1;
                master.DeletedAt = DateTime.UtcNow;
                await _newsPaperRepo.UpdateAsync(master);
            }

            return (true, "Record deleted successfully.");
        }

        // ── Publish ────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> PublishAsync(int id)
        {
            var e = await _clientNewsPaperRepo.GetByIdAsync(id);
            if (e == null) return (false, "Record not found.");
            e.Publish = true; e.UpdatedAt = DateTime.UtcNow;
            await _clientNewsPaperRepo.UpdateAsync(e);
            return (true, "Published.");
        }

        public async Task<(bool Success, string Message)> UnpublishAsync(int id)
        {
            var e = await _clientNewsPaperRepo.GetByIdAsync(id);
            if (e == null) return (false, "Record not found.");
            e.Publish = false; e.UpdatedAt = DateTime.UtcNow;
            await _clientNewsPaperRepo.UpdateAsync(e);
            return (true, "Unpublished.");
        }

        // ── Dropdown builders ──────────────────────────────────────────────────

        public async Task<List<MediaSelectOption>> GetPublicationOptionsAsync(int selectedId = 0)
        {
            var pubs = await _publicationRepo.GetActivePublicationsAsync();
            return pubs.Select(p => new MediaSelectOption
            {
                Value = p.Id.ToString(),
                Text = p.PublicationName,
                Selected = p.Id == selectedId
            }).ToList();
        }

        public async Task<List<MediaSelectOption>> GetCategoryOptionsAsync(int clientId, int selectedId = 0)
        {
            var cats = await _context.Set<ClientCategories>()
                .Where(c => c.ClientId == clientId && c.ParentCategory == null
                         && c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.Order).ThenBy(c => c.CategoryName)
                .ToListAsync();
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
                .OrderBy(c => c.Order).ThenBy(c => c.CategoryName)
                .ToListAsync();
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

        public async Task<PublicationAutoFillDTO> GetPublicationAutoFillAsync(
            int publicationId, int clientId)
        {
            var pub = await _publicationRepo.GetByIdAsync(publicationId);
            if (pub == null) return new PublicationAutoFillDTO();

            // MediaTier comes from PublicationCustomerCategory for this client
            var pcc = await _context.Set<PublicationCustomerCategory>()
                .FirstOrDefaultAsync(p => p.PublicationId == publicationId
                                       && p.CustomerId == clientId);

            var adValue = pub.CmPrice;
            var circulation = pub.Circulation ?? 0;

            return new PublicationAutoFillDTO
            {
                AdValue = adValue,
                PrValue = Math.Round(adValue * 3.5m, 2),
                MediaType = pub.MediaType,
                MediaTier = pcc?.MediaTier ?? pub.MediaTier,
                Frequency = pub.Frequency,
                Language = pub.Language,
                Circulation = pub.Circulation,
                Reach = circulation * 4
            };
        }

        // ── Mapper ─────────────────────────────────────────────────────────────

        private static ClientNewsPaperDTO MapToDTO(ClientNewsPaper e) => new()
        {
            Id = e.Id,
            NewsPaperId = e.NewsPaperId,
            ClientId = e.ClientId,
            ClientName = e.Client?.Name,
            PublicationId = e.PublicationId,
            CategoryId = e.CategoryId,
            SubCategoryId = e.SubCategoryId,
            WriterId = e.WriterId,
            WriterName = e.Writer?.WriterName,
            Date = e.Date,
            Title = e.Title,
            Pages = e.Pages,
            Height = e.Height,
            Width = e.Width,
            ADValue = e.ADValue,
            PRValue = e.PRValue,
            ArticleBranding = e.ArticleBranding,
            HeadlineBranding = e.HeadlineBranding,
            Toning = e.Toning,
            Content = e.Content,
           // Images = e.Images,
            Publish = e.Publish
        };
    }
}
