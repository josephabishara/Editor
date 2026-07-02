using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.General;
using EditorViewModelLayer.NewsViewModel;
using EditorViewModelLayer.ReportViewModel;
using System.Text.Json;

namespace EditorLogicLayer.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepo;
        private readonly IReportArticleRepository _reportArticleRepo;
        private readonly IReportNewspaperRepository _reportNewspaperRepo;
        private readonly IClientRepository _clientRepo;
        private readonly IClientArticleRepository _clientArticleRepo;
        private readonly IClientNewsPaperRepository _clientNewsPaperRepo;
        private readonly IClientCategoryRepository _categoryRepo;
        private readonly IPublicationRepository _publicationRepo;
        private readonly IWebsiteRepository _websiteRepo;
        private readonly IWriterRepository _writerRepo;

        public ReportService(
            IReportRepository reportRepo,
            IReportArticleRepository reportArticleRepo,
            IReportNewspaperRepository reportNewspaperRepo,
            IClientRepository clientRepo,
            IClientArticleRepository clientArticleRepo,
            IClientNewsPaperRepository clientNewsPaperRepo,
            IClientCategoryRepository categoryRepo,
            IPublicationRepository publicationRepo,
            IWebsiteRepository websiteRepo,
            IWriterRepository writerRepo)
        {
            _reportRepo = reportRepo;
            _reportArticleRepo = reportArticleRepo;
            _reportNewspaperRepo = reportNewspaperRepo;
            _clientRepo = clientRepo;
            _clientArticleRepo = clientArticleRepo;
            _clientNewsPaperRepo = clientNewsPaperRepo;
            _categoryRepo = categoryRepo;
            _publicationRepo = publicationRepo;
            _websiteRepo = websiteRepo;
            _writerRepo = writerRepo;
        }

        // ── List ────────────────────────────────────────────────────────────────

        public async Task<IEnumerable<ReportDTO>> GetAllAsync()
        {
            var list = await _reportRepo.GetActiveAsync();
            return list.Select(MapToDTO);
        }

        // ── Single ──────────────────────────────────────────────────────────────

        public async Task<ReportDTO?> GetByIdAsync(int id)
        {
            var entity = await _reportRepo.GetByIdAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<ReportDetailsDTO?> GetDetailsAsync(int id)
        {
            var entity = await _reportRepo.GetByIdWithNavAsync(id);
            if (entity == null) return null;

            return await MapToDetailsDTOAsync(entity);
        }

        // ── Create ──────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message, int ReportId)> CreateAsync(ReportDTO model)
        {
            var entity = new Report
            {
                CustomerId = model.CustomerId,
                ReportName = model.ReportName,
                ReportDate = model.ReportDate,
                ReportType = model.ReportType,
                Publish = false,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _reportRepo.AddAsync(entity);
            return (true, "Report created successfully.", entity.Id);
        }

        // ── Update ──────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> UpdateAsync(ReportDTO model)
        {
            var existing = await _reportRepo.GetByIdAsync(model.Id);
            if (existing == null) return (false, "Report not found.");

            existing.CustomerId = model.CustomerId;
            existing.ReportName = model.ReportName;
            existing.ReportDate = model.ReportDate;
            existing.ReportType = model.ReportType;
            existing.UpdatedAt = DateTime.UtcNow;

            await _reportRepo.UpdateAsync(existing);
            return (true, "Report updated successfully.");
        }

        // ── Delete ──────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _reportRepo.GetByIdAsync(id);
            if (existing == null) return (false, "Report not found.");

            existing.Deleted = 1;
            existing.IsActive = false;
            existing.DeletedAt = DateTime.UtcNow;

            await _reportRepo.UpdateAsync(existing);
            return (true, "Report deleted successfully.");
        }

        // ── Publish / UnPublish ─────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> PublishAsync(int id)
        {
            var existing = await _reportRepo.GetByIdAsync(id);
            if (existing == null) return (false, "Report not found.");

            existing.Publish = true;
            existing.UpdatedAt = DateTime.UtcNow;

            await _reportRepo.UpdateAsync(existing);
            return (true, "Report published successfully.");
        }

        public async Task<(bool Success, string Message)> UnPublishAsync(int id)
        {
            var existing = await _reportRepo.GetByIdAsync(id);
            if (existing == null) return (false, "Report not found.");

            existing.Publish = false;
            existing.UpdatedAt = DateTime.UtcNow;

            await _reportRepo.UpdateAsync(existing);
            return (true, "Report unpublished successfully.");
        }

        // ── Wizard Step 2: Articles — filtered to the report's client ───────────

        public async Task<List<ReportArticlePickerDTO>> GetArticlePickerAsync(
            int reportId, DateTime? from, DateTime? to)
        {
            var report = await _reportRepo.GetByIdAsync(reportId);
            if (report == null) return new List<ReportArticlePickerDTO>();

            // ── Source: ClientArticle filtered to THIS client (not GeneralArticle) ──
            var clientArticles = await _clientArticleRepo.GetByClientIdAsync(report.CustomerId, from, to);

            var categoryMap = await BuildCategoryMapAsync(report.CustomerId);

            // Bulk lookups — same pattern as publication/category maps, avoids N+1 queries
            var websiteMap = (await _websiteRepo.GetActiveWebsitesAsync())
                              .ToDictionary(w => w.Id, w => w.WebsiteName);

            var writerMap = (await _writerRepo.GetActiveWritersAsync())
                             .ToDictionary(w => w.Id, w => w.WriterName);

            var existingLinks = (await _reportArticleRepo.GetByReportIdAsync(reportId))
                                 .Select(ra => ra.ArticleId)
                                 .ToHashSet();

            return clientArticles
                .Select(ca => MapClientArticleToPicker(ca, categoryMap, websiteMap, writerMap, existingLinks))
                .OrderBy(a => a.CategoryOrder).ThenBy(a => a.CategoryName)
                .ThenBy(a => a.SubCategoryOrder).ThenBy(a => a.SubCategoryName)
                .ThenByDescending(a => a.Date)
                .ToList();
        }

        public async Task<(bool Success, string Message)> SaveArticlesAsync(
            int reportId, List<int> articleIds)
        {
            var report = await _reportRepo.GetByIdAsync(reportId);
            if (report == null) return (false, "Report not found.");

            await _reportArticleRepo.DeleteByReportIdAsync(reportId);

            if (articleIds.Any())
            {
                var rows = articleIds.Select(aid => new ReportArticle
                {
                    ReportId = reportId,
                    ArticleId = aid,
                    IsActive = true,
                    Deleted = 0,
                    CreatedAt = DateTime.UtcNow
                });
                await _reportArticleRepo.AddRangeAsync(rows);
            }

            return (true, "Articles saved successfully.");
        }

        // ── Wizard Step 3: Newspapers — filtered to the report's client ─────────

        public async Task<List<ReportNewspaperPickerDTO>> GetNewspaperPickerAsync(
            int reportId, DateTime? from, DateTime? to)
        {
            var report = await _reportRepo.GetByIdAsync(reportId);
            if (report == null) return new List<ReportNewspaperPickerDTO>();

            // ── Source: ClientNewsPaper filtered to THIS client (not NewsPaper) ─────
            var clientNewsPapers = await _clientNewsPaperRepo.GetByClientIdAsync(report.CustomerId, from, to);

            var categoryMap = await BuildCategoryMapAsync(report.CustomerId);

            var publications = (await _publicationRepo.GetActivePublicationsAsync())
                                .ToDictionary(p => p.Id, p => p.PublicationName);

            var existingLinks = (await _reportNewspaperRepo.GetByReportIdAsync(reportId))
                                 .Select(rn => rn.NewspaperId)
                                 .ToHashSet();

            return clientNewsPapers
                .Select(cn => MapClientNewsPaperToPicker(cn, categoryMap, publications, existingLinks))
                .OrderBy(n => n.CategoryOrder).ThenBy(n => n.CategoryName)
                .ThenBy(n => n.SubCategoryOrder).ThenBy(n => n.SubCategoryName)
                .ThenByDescending(n => n.Date)
                .ToList();
        }

        public async Task<(bool Success, string Message)> SaveNewspapersAsync(
            int reportId, List<int> newspaperIds)
        {
            var report = await _reportRepo.GetByIdAsync(reportId);
            if (report == null) return (false, "Report not found.");

            await _reportNewspaperRepo.DeleteByReportIdAsync(reportId);

            if (newspaperIds.Any())
            {
                var rows = newspaperIds.Select(nid => new ReportNewspaper
                {
                    ReportId = reportId,
                    NewspaperId = nid,
                    IsActive = true,
                    Deleted = 0,
                    CreatedAt = DateTime.UtcNow
                });
                await _reportNewspaperRepo.AddRangeAsync(rows);
            }

            return (true, "Newspapers saved successfully.");
        }

        // ── Dropdowns ───────────────────────────────────────────────────────────

        public async Task<IEnumerable<SelectOption>> GetClientOptionsAsync()
        {
            var clients = await _clientRepo.GetActiveClientsAsync();
            return clients.Select(c => new SelectOption
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });
        }

        // ── Mappers ─────────────────────────────────────────────────────────────

        private static ReportDTO MapToDTO(Report r) => new()
        {
            Id = r.Id,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.Name,
            ReportName = r.ReportName,
            ReportDate = r.ReportDate,
            Publish = r.Publish,
            ReportType = r.ReportType,
            ArticleCount = r.ReportArticles?.Count ?? 0,
            NewspaperCount = r.ReportNewspapers?.Count ?? 0
        };

        /// <summary>
        /// Resolves Category/SubCategory names + sort order for a client in one query,
        /// keyed by ClientCategories.Id. Reused for both Articles and Newspapers.
        /// </summary>
        private async Task<Dictionary<int, ClientCategories>> BuildCategoryMapAsync(int clientId)
        {
            var categories = await _categoryRepo.GetByClientAsync(clientId);
            return categories.ToDictionary(c => c.Id);
        }

        private static ReportArticlePickerDTO MapClientArticleToPicker(
            ClientArticle ca,
            Dictionary<int, ClientCategories> categoryMap,
            Dictionary<int, string> websiteMap,
            Dictionary<int, string> writerMap,
            HashSet<int> existingLinks)
        {
            categoryMap.TryGetValue(ca.CategoryId, out var category);
            categoryMap.TryGetValue(ca.SubCategoryId, out var subCategory);
            websiteMap.TryGetValue(ca.WebsiteId, out var websiteName);  // resolve via FK, not a nav property
            writerMap.TryGetValue(ca.WriterId, out var writerName);      // ca.Writer scalar is always null; resolve via WriterId FK

            return new ReportArticlePickerDTO
            {
                ArticleId = ca.Id,
                Date = ca.Date,
                Title = ca.Title,
                ArticleURL = ca.ArticleURL,
                WebsiteName = websiteName,
                WriterName = writerName,
                Language = ca.Language,
                ArticleBranding = ca.ArticleBranding,
                HeadlineBranding = ca.HeadlineBranding,
                MediaTier = ca.MediaTier,
                MediaType = ca.MediaType,
                CategoryId = ca.CategoryId,
                CategoryName = category?.CategoryName ?? "Uncategorized",
                CategoryOrder = category?.Order ?? int.MaxValue,
                SubCategoryId = ca.SubCategoryId,
                SubCategoryName = subCategory?.CategoryName,
                SubCategoryOrder = subCategory?.Order ?? int.MaxValue,
                Content = ca.Content,
                ImagePaths = ParseImages(ca.Images),
                PRValue = ca.PRValue ?? 0m,
                ADValue = ca.ADValue ?? 0m,
                Selected = existingLinks.Contains(ca.Id),
                Frequency = ca.Frequency,
                Impression = ca.Impression,
                Reach = ca.Reach,
                Toning = ca.Toning,
                PictureinArticle = ca.PictureinArticle,
                Generation = ca.Generation.ToString(),
            };
        }

        private static ReportNewspaperPickerDTO MapClientNewsPaperToPicker(
            ClientNewsPaper cn,
            Dictionary<int, ClientCategories> categoryMap,
            Dictionary<int, string?> publications,
            HashSet<int> existingLinks)
        {
            categoryMap.TryGetValue(cn.CategoryId, out var category);
            categoryMap.TryGetValue(cn.SubCategoryId, out var subCategory);
            publications.TryGetValue(cn.PublicationId, out var pubName);

            return new ReportNewspaperPickerDTO
            {
                NewspaperId = cn.Id,
                Date = cn.Date,
                Title = cn.Title,
                PublicationId = cn.PublicationId,
                PublicationName = pubName,
                WriterName = cn.Writer?.WriterName,
                CategoryId = cn.CategoryId,
                CategoryName = category?.CategoryName ?? "Uncategorized",
                CategoryOrder = category?.Order ?? int.MaxValue,
                SubCategoryId = cn.SubCategoryId,
                SubCategoryName = subCategory?.CategoryName,
                SubCategoryOrder = subCategory?.Order ?? int.MaxValue,
                Content = cn.Content,
                ImagePaths = ParseImages(cn.Images),   // ClientNewsPaper.Images — same JSON-array convention as ClientArticle.Images
                PRValue = cn.PRValue ?? 0m,
                ADValue = cn.ADValue ?? 0m,
                ArticleBranding = cn.ArticleBranding,
                HeadlineBranding = cn.HeadlineBranding,
                Toning = cn.Toning,
                MediaType = cn.MediaType,
                MediaTier = cn.MediaTier,
                Frequency = cn.Frequency,
                Circulation = cn.Circulation,
                Reach = cn.Reach,
                PictureinArticle = cn.pictureInArticle.ToString(),
                PageNumber = cn.Pages,
                Generation = cn.Generation,
                Selected = existingLinks.Contains(cn.Id),
                Height = cn.Height,
                Width = cn.Width,
                Language = cn.Language
            };
        }

        /// <summary>
        /// ClientArticle.Images is stored as a JSON-serialized List&lt;string&gt; of relative
        /// paths (see ClientArticleController.SaveImagesAsync). Deserializes defensively.
        /// </summary>
        private static List<string> ParseImages(string? imagesJson)
        {
            if (string.IsNullOrWhiteSpace(imagesJson)) return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(imagesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Async mapper for the Details/Preview pages: resolves Category/SubCategory names,
        /// PR/AD values, Content and Images from ClientArticle/ClientNewsPaper, then groups
        /// and sorts everything by Category → SubCategory → Date for the printable report.
        /// </summary>
        private async Task<ReportDetailsDTO> MapToDetailsDTOAsync(Report r)
        {
            var categoryMap = await BuildCategoryMapAsync(r.CustomerId);

            var clientArticles = (await _clientArticleRepo.GetByClientIdAsync(r.CustomerId, null, null))
                                    .ToDictionary(ca => ca.Id);

            var clientNewsPapers = (await _clientNewsPaperRepo.GetByClientIdAsync(r.CustomerId, null, null))
                                    .ToDictionary(cn => cn.Id);

            var publications = (await _publicationRepo.GetActivePublicationsAsync())
                                .ToDictionary(p => p.Id, p => p.PublicationName);

            // Bulk lookups for article WebsiteName and WriterName — same pattern as publications
            var websiteMap = (await _websiteRepo.GetActiveWebsitesAsync())
                              .ToDictionary(w => w.Id, w => w.WebsiteName);

            var writerMap = (await _writerRepo.GetActiveWritersAsync())
                             .ToDictionary(w => w.Id, w => w.WriterName);

            // ── Client cover/logo for the Preview page ───────────────────────────
            var client = r.Customer ?? await _clientRepo.GetByIdAsync(r.CustomerId);

            var dto = new ReportDetailsDTO
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                CustomerName = client?.Name,
                CustomerLogoUrl = client?.Photo,
                CustomerReportCoverPdfUrl = client?.ReportCoverPdf,
                ReportName = r.ReportName,
                ReportDate = r.ReportDate,
                Publish = r.Publish,
                ReportType = r.ReportType
            };

            // ── Articles — ReportArticle.ArticleId stores ClientArticle.Id (the row's own
            //    primary key), NOT the original GeneralArticle/source ArticleId column.
            //    This must stay consistent with MapClientArticleToPicker, which sets
            //    ArticleId = ca.Id for the same reason. ──────────────────────────────
            var existingArticleLinks = r.ReportArticles.Select(ra => ra.ArticleId).ToHashSet();

            dto.Articles = r.ReportArticles
                .Where(ra => clientArticles.ContainsKey(ra.ArticleId))
                .Select(ra => MapClientArticleToPicker(clientArticles[ra.ArticleId], categoryMap, websiteMap, writerMap, existingArticleLinks))
                .ToList();

            // ── Newspapers — same convention: ReportNewspaper.NewspaperId stores
            //    ClientNewsPaper.Id, not NewsPaper.Id. ─────────────────────────────────
            var existingNewsLinks = r.ReportNewspapers.Select(rn => rn.NewspaperId).ToHashSet();

            dto.Newspapers = r.ReportNewspapers
                .Where(rn => clientNewsPapers.ContainsKey(rn.NewspaperId))
                .Select(rn => MapClientNewsPaperToPicker(clientNewsPapers[rn.NewspaperId], categoryMap, publications, existingNewsLinks))
                .ToList();

            dto.ArticleCount = dto.Articles.Count;
            dto.NewspaperCount = dto.Newspapers.Count;

            // ── Group by Category → SubCategory, sorted, for the Preview page ────
            dto.CategoryGroups = BuildCategoryGroups(dto.Articles, dto.Newspapers);

            return dto;
        }

        /// <summary>
        /// Groups articles + newspapers by (CategoryName, SubCategoryName), ordered by
        /// ClientCategories.Order then by name. One group = one page-break section
        /// with a Category heading in the Preview view.
        /// </summary>
        private static List<ReportCategoryGroupDTO> BuildCategoryGroups(
            List<ReportArticlePickerDTO> articles,
            List<ReportNewspaperPickerDTO> newspapers)
        {
            var groupKeys = articles
                .Select(a => (a.CategoryName, a.SubCategoryName, a.CategoryOrder, a.SubCategoryOrder))
                .Concat(newspapers.Select(n => (n.CategoryName, n.SubCategoryName, n.CategoryOrder, n.SubCategoryOrder)))
                .Distinct()
                .OrderBy(k => k.CategoryOrder).ThenBy(k => k.CategoryName)
                .ThenBy(k => k.SubCategoryOrder).ThenBy(k => k.SubCategoryName)
                .ToList();

            var groups = new List<ReportCategoryGroupDTO>();

            foreach (var key in groupKeys)
            {
                groups.Add(new ReportCategoryGroupDTO
                {
                    CategoryName = key.CategoryName ?? "Uncategorized",
                    SubCategoryName = key.SubCategoryName,
                    Articles = articles
                        .Where(a => a.CategoryName == key.CategoryName && a.SubCategoryName == key.SubCategoryName)
                        .OrderByDescending(a => a.Date)
                        .ToList(),
                    Newspapers = newspapers
                        .Where(n => n.CategoryName == key.CategoryName && n.SubCategoryName == key.SubCategoryName)
                        .OrderByDescending(n => n.Date)
                        .ToList()
                });
            }

            return groups;
        }
    }
}
