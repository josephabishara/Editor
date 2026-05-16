using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.NewsViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.News
{
    public class ClientNewsService : IClientNewsService
    {
        private readonly IClientNewsRepository _clientNewsRepo;
        private readonly INewsRepository _newsRepo;
        private readonly IClientRepository _clientRepo;
        private readonly IPublicationRepository _publicationRepo;
        private readonly IWebsiteRepository _websiteRepo;
        private readonly IChannelRepository _channelRepo;
        private readonly IWriterRepository _writerRepo;
        private readonly ApplicationDbContext _context;

        public ClientNewsService(
            IClientNewsRepository clientNewsRepo,
            INewsRepository newsRepo,
            IClientRepository clientRepo,
            IPublicationRepository publicationRepo,
            IWebsiteRepository websiteRepo,
            IChannelRepository channelRepo,
            IWriterRepository writerRepo,
            ApplicationDbContext context)
        {
            _clientNewsRepo = clientNewsRepo;
            _newsRepo = newsRepo;
            _clientRepo = clientRepo;
            _publicationRepo = publicationRepo;
            _websiteRepo = websiteRepo;
            _channelRepo = channelRepo;
            _writerRepo = writerRepo;
            _context = context;
        }

        // ── List ───────────────────────────────────────────────────────────────

        public async Task<ClientNewsListDTO> GetClientNewsDashboardAsync(int clientId)
        {
            var client = await _clientRepo.GetByIdAsync(clientId);
            var items = await _clientNewsRepo.GetByClientIdAsync(clientId);
            return new ClientNewsListDTO
            {
                ClientId = clientId,
                ClientName = client?.Name ?? string.Empty,
                ClientPhoto = client?.Photo,
                Items = items.Select(MapToDTO).ToList()
            };
        }

        public async Task<IEnumerable<ClientNewsDTO>> GetByClientIdAsync(int clientId)
        {
            var items = await _clientNewsRepo.GetByClientIdAsync(clientId);
            return items.Select(MapToDTO);
        }

        public async Task<ClientNewsDTO?> GetByIdAsync(int clientNewsId)
        {
            var cn = await _clientNewsRepo.GetByIdWithDetailsAsync(clientNewsId);
            return cn == null ? null : MapToDTO(cn);
        }

        // ── Create ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> CreateAsync(ClientNewsDTO model)
        {
            if (model.NewsMode == "Existing")
                return await CreateFromExistingAsync(model);
            return await CreateNewAsync(model);
        }

        private async Task<(bool Success, string Message)> CreateNewAsync(ClientNewsDTO model)
        {
            var client = await _clientRepo.GetByIdAsync(model.ClientId);
            if (client == null) return (false, "Client not found.");

            // Step 1: insert ClientNews (NewsId = 0 temporarily)
            var clientNews = MapToEntity(model);
            clientNews.NewsId = 0;
            clientNews.IsActive = true;
            clientNews.Deleted = 0;
            clientNews.CreatedAt = DateTime.UtcNow;
            await _clientNewsRepo.AddAsync(clientNews);

            // Step 2: create News master as copy
            var news = new EditorEntitiesLayer.Entities.News
            {
                SourceType = model.SourceType,
                Date = clientNews.Date,
                Title = clientNews.Title,
                PRValue = clientNews.PRValue,
                ADValue = clientNews.ADValue,
                PROption = clientNews.PROption,
                ADOption = clientNews.ADOption,
                ArticleBranding = clientNews.ArticleBranding,
                HeadlineBranding = clientNews.HeadlineBranding,
                pictureInArticle = clientNews.pictureInArticle,
                Generation = clientNews.Generation,
                Toning = clientNews.Toning,
                Translation = clientNews.Translation,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _newsRepo.AddAsync(news);

            // Step 3: link
            clientNews.NewsId = news.Id;
            clientNews.UpdatedAt = DateTime.UtcNow;
            await _clientNewsRepo.UpdateAsync(clientNews);

            return (true, "News created successfully.");
        }

        private async Task<(bool Success, string Message)> CreateFromExistingAsync(ClientNewsDTO model)
        {
            if (!model.ExistingNewsId.HasValue || model.ExistingNewsId == 0)
                return (false, "Please select an existing news record.");

            var existingNews = await _newsRepo.GetByIdAsync(model.ExistingNewsId.Value);
            if (existingNews == null) return (false, "Selected news record not found.");

            var already = await _clientNewsRepo.GetByNewsIdAsync(existingNews.Id);
            if (already.Any(cn => cn.ClientId == model.ClientId))
                return (false, "This news is already assigned to this client.");

            var clientNews = new ClientNews
            {
                NewsId = existingNews.Id,
                ClientId = model.ClientId,
                publicationId = model.publicationId,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                WriterId = model.WriterId,
                Pages = model.Pages,
                Height = model.Height,
                Width = model.Width,
                Date = existingNews.Date,
                Title = existingNews.Title,
                PRValue = existingNews.PRValue,
                ADValue = existingNews.ADValue,
                PROption = existingNews.PROption,
                ADOption = existingNews.ADOption,
                ArticleBranding = existingNews.ArticleBranding,
                HeadlineBranding = existingNews.HeadlineBranding,
                pictureInArticle = existingNews.pictureInArticle,
                Generation = existingNews.Generation,
                Toning = existingNews.Toning,
                Translation = existingNews.Translation,
                Publish = false,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _clientNewsRepo.AddAsync(clientNews);
            return (true, $"News '{existingNews.Title}' assigned to client successfully.");
        }

        // ── Update ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> UpdateAsync(ClientNewsDTO model)
        {
            var cn = await _clientNewsRepo.GetByIdWithDetailsAsync(model.Id);
            if (cn == null) return (false, "Record not found.");

            cn.publicationId = model.publicationId;
            cn.CategoryId = model.CategoryId;
            cn.SubCategoryId = model.SubCategoryId;
            cn.WriterId = model.WriterId;
            cn.Pages = model.Pages;
            cn.Height = model.Height;
            cn.Width = model.Width;
            cn.Date = model.Date;
            cn.Title = model.Title;
            cn.PRValue = model.PRValue;
            cn.ADValue = model.ADValue;
            cn.PROption = model.PROption;
            cn.ADOption = model.ADOption;
            cn.ArticleBranding = model.ArticleBranding;
            cn.HeadlineBranding = model.HeadlineBranding;
            cn.pictureInArticle = model.pictureInArticle;
            cn.Generation = model.Generation;
            cn.Toning = model.Toning;
            cn.Translation = model.Translation;
            cn.UpdatedAt = DateTime.UtcNow;

            await _clientNewsRepo.UpdateAsync(cn);
            return (true, "News updated successfully.");
        }

        // ── Delete ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> DeleteAsync(int clientNewsId)
        {
            var cn = await _clientNewsRepo.GetByIdWithDetailsAsync(clientNewsId);
            if (cn == null) return (false, "Record not found.");

            cn.IsActive = false;
            cn.Deleted = 1;
            cn.DeletedAt = DateTime.UtcNow;
            await _clientNewsRepo.UpdateAsync(cn);

            var others = await _clientNewsRepo.GetByNewsIdAsync(cn.NewsId);
            if (!others.Any(o => o.Id != cn.Id))
            {
                var news = cn.News;
                news.IsActive = false;
                news.Deleted = 1;
                news.DeletedAt = DateTime.UtcNow;
                await _newsRepo.UpdateAsync(news);
            }
            return (true, "News deleted successfully.");
        }

        // ── Publish ────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> PublishAsync(int id)
        {
            var cn = await _clientNewsRepo.GetByIdAsync(id);
            if (cn == null) return (false, "Record not found.");
            cn.Publish = true; cn.UpdatedAt = DateTime.UtcNow;
            await _clientNewsRepo.UpdateAsync(cn);
            return (true, "News published.");
        }

        public async Task<(bool Success, string Message)> UnpublishAsync(int id)
        {
            var cn = await _clientNewsRepo.GetByIdAsync(id);
            if (cn == null) return (false, "Record not found.");
            cn.Publish = false; cn.UpdatedAt = DateTime.UtcNow;
            await _clientNewsRepo.UpdateAsync(cn);
            return (true, "News unpublished.");
        }

        // ── SelectListItem builders ────────────────────────────────────────────

        public async Task<List<SelectListItem>> GetSourceSelectListAsync(
            string sourceType, int selectedId = 0)
        {
            return sourceType switch
            {
                "Publication" => (await _publicationRepo.GetActivePublicationsAsync())
                    .Select(p => new SelectListItem(
                        $"{p.PublicationName}{(p.MediaTier != null ? $" ({p.MediaTier})" : "")}",
                        p.Id.ToString(),
                        p.Id == selectedId))
                    .ToList(),

                "Article" => (await _websiteRepo.GetActiveWebsitesAsync())
                    .Select(w => new SelectListItem(
                        $"{w.WebsiteName}{(w.MediaTier != null ? $" ({w.MediaTier})" : "")}",
                        w.Id.ToString(),
                        w.Id == selectedId))
                    .ToList(),

                "Video" => (await _channelRepo.GetActiveChannelsAsync())
                    .Select(c => new SelectListItem(
                        $"{c.ChannelName}{(c.MediaTier != null ? $" ({c.MediaTier})" : "")}",
                        c.Id.ToString(),
                        c.Id == selectedId))
                    .ToList(),

                _ => new List<SelectListItem>()
            };
        }

        public async Task<List<SelectListItem>> GetCategorySelectListAsync(
            int clientId, int selectedId = 0)
        {
            var cats = await _context.Set<ClientCategories>()
                .Where(c => c.ClientId == clientId
                         && c.ParentCategory == null
                         && c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.Order).ThenBy(c => c.CategoryName)
                .ToListAsync();

            return cats.Select(c => new SelectListItem(
                c.CategoryName, c.Id.ToString(), c.Id == selectedId))
                .ToList();
        }

        public async Task<List<SelectListItem>> GetSubCategorySelectListAsync(
            int parentId, int selectedId = 0)
        {
            if (parentId == 0) return new List<SelectListItem>();

            var cats = await _context.Set<ClientCategories>()
                .Where(c => c.ParentCategory == parentId
                         && c.IsActive && c.Deleted == 0)
                .OrderBy(c => c.Order).ThenBy(c => c.CategoryName)
                .ToListAsync();

            return cats.Select(c => new SelectListItem(
                c.CategoryName, c.Id.ToString(), c.Id == selectedId))
                .ToList();
        }

        public async Task<List<SelectListItem>> GetWriterSelectListAsync(int selectedId = 0)
        {
            var writers = await _writerRepo.GetActiveWritersAsync();
            return writers.Select(w => new SelectListItem(
                w.WriterName, w.Id.ToString(), w.Id == selectedId))
                .ToList();
        }

        public async Task<List<SelectListItem>> GetExistingNewsSelectListAsync(
            string sourceType, int selectedId = 0)
        {
            var news = string.IsNullOrWhiteSpace(sourceType)
                ? await _newsRepo.GetActiveNewsAsync()
                : await _newsRepo.GetBySourceTypeAsync(sourceType);

            return news.Select(n => new SelectListItem(
                $"[{n.SourceType}] {n.Date:dd MMM yyyy} — {n.Title}",
                n.Id.ToString(),
                n.Id == selectedId))
                .ToList();
        }

        public async Task<ClientNewsDTO?> PrefillFromExistingNewsAsync(int newsId, int clientId)
        {
            var news = await _newsRepo.GetByIdAsync(newsId);
            if (news == null) return null;
            return new ClientNewsDTO
            {
                ClientId = clientId,
                NewsId = newsId,
                NewsMode = "Existing",
                ExistingNewsId = newsId,
                SourceType = news.SourceType,
                Date = news.Date,
                Title = news.Title,
                PRValue = news.PRValue,
                ADValue = news.ADValue,
                PROption = news.PROption,
                ADOption = news.ADOption,
                ArticleBranding = news.ArticleBranding,
                HeadlineBranding = news.HeadlineBranding,
                pictureInArticle = news.pictureInArticle,
                Generation = news.Generation,
                Toning = news.Toning,
                Translation = news.Translation,
            };
        }

        // ── Mapper ─────────────────────────────────────────────────────────────

        private static ClientNewsDTO MapToDTO(ClientNews cn) => new()
        {
            Id = cn.Id,
            NewsId = cn.NewsId,
            ClientId = cn.ClientId,
            ClientName = cn.Client?.Name,
            SourceType = cn.News?.SourceType ?? "Publication",
            publicationId = cn.publicationId,
            CategoryId = cn.CategoryId,
            SubCategoryId = cn.SubCategoryId,
            WriterId = cn.WriterId,
            WriterName = cn.Writer?.WriterName,
            Pages = cn.Pages,
            Height = cn.Height,
            Width = cn.Width,
            Date = cn.Date,
            Title = cn.Title,
            PRValue = cn.PRValue,
            ADValue = cn.ADValue,
            PROption = cn.PROption,
            ADOption = cn.ADOption,
            ArticleBranding = cn.ArticleBranding,
            HeadlineBranding = cn.HeadlineBranding,
            pictureInArticle = cn.pictureInArticle,
            Generation = cn.Generation,
            Toning = cn.Toning,
            Translation = cn.Translation,
            Publish = cn.Publish
        };

        private static ClientNews MapToEntity(ClientNewsDTO dto) => new()
        {
            ClientId = dto.ClientId,
            publicationId = dto.publicationId,
            CategoryId = dto.CategoryId,
            SubCategoryId = dto.SubCategoryId,
            WriterId = dto.WriterId,
            Pages = dto.Pages,
            Height = dto.Height,
            Width = dto.Width,
            Date = dto.Date,
            Title = dto.Title,
            PRValue = dto.PRValue,
            ADValue = dto.ADValue,
            PROption = dto.PROption,
            ADOption = dto.ADOption,
            ArticleBranding = dto.ArticleBranding,
            HeadlineBranding = dto.HeadlineBranding,
            pictureInArticle = dto.pictureInArticle,
            Generation = dto.Generation,
            Toning = dto.Toning,
            Translation = dto.Translation,
            Publish = false
        };
    }
}
