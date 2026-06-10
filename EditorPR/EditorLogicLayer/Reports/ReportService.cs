using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.General;
using EditorViewModelLayer.NewsViewModel;
using EditorViewModelLayer.ReportViewModel;

namespace EditorLogicLayer.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepo;
        private readonly IReportArticleRepository _reportArticleRepo;
        private readonly IReportNewspaperRepository _reportNewspaperRepo;
        private readonly IGeneralArticleRepository _articleRepo;
        private readonly INewsPaperRepository _newspaperRepo;
        private readonly IClientRepository _clientRepo;
        private readonly IClientArticleRepository _clientArticleRepo;
        private readonly IClientNewsPaperRepository _clientNewsPaperRepo;
        private readonly IPublicationRepository _publicationRepo;

        public ReportService(
            IReportRepository reportRepo,
            IReportArticleRepository reportArticleRepo,
            IReportNewspaperRepository reportNewspaperRepo,
            IGeneralArticleRepository articleRepo,
            INewsPaperRepository newspaperRepo,
            IClientRepository clientRepo,
            IClientArticleRepository clientArticleRepo,
            IClientNewsPaperRepository clientNewsPaperRepo,
            IPublicationRepository publicationRepo)
        {
            _reportRepo = reportRepo;
            _reportArticleRepo = reportArticleRepo;
            _reportNewspaperRepo = reportNewspaperRepo;
            _articleRepo = articleRepo;
            _newspaperRepo = newspaperRepo;
            _clientRepo = clientRepo;
            _clientArticleRepo = clientArticleRepo;
            _clientNewsPaperRepo = clientNewsPaperRepo;
            _publicationRepo = publicationRepo;
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

        // ── Wizard Step 2: Articles ─────────────────────────────────────────────

        public async Task<List<ReportArticlePickerDTO>> GetArticlePickerAsync(
            int reportId, DateTime? from, DateTime? to)
        {
            var all = await _articleRepo.GetActiveAsync();

            IEnumerable<EditorEntitiesLayer.Entities.GeneralArticle> filtered = all;
            if (from.HasValue)
                filtered = filtered.Where(a => a.Date >= from.Value);
            if (to.HasValue)
                filtered = filtered.Where(a => a.Date <= to.Value.AddDays(1).AddTicks(-1));

            var existing = (await _reportArticleRepo.GetByReportIdAsync(reportId))
                           .Select(ra => ra.ArticleId)
                           .ToHashSet();

            return filtered.Select(a => new ReportArticlePickerDTO
            {
                ArticleId = a.Id,
                Date = a.Date,
                Title = a.Title,
                ArticleURL = a.ArticleURL,
                WebsiteName = a.Website?.WebsiteName,
                WriterName = a.Writer?.WriterName,
                Language = a.Language,
                ArticleBranding = a.ArticleBranding,
                Selected = existing.Contains(a.Id)
            }).ToList();
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

        // ── Wizard Step 3: Newspapers ───────────────────────────────────────────

        public async Task<List<ReportNewspaperPickerDTO>> GetNewspaperPickerAsync(
            int reportId, DateTime? from, DateTime? to)
        {
            var all = await _newspaperRepo.GetActiveAsync();

            IEnumerable<NewsPaper> filtered = all;
            if (from.HasValue)
                filtered = filtered.Where(n => n.Date >= from.Value);
            if (to.HasValue)
                filtered = filtered.Where(n => n.Date <= to.Value.AddDays(1).AddTicks(-1));

            var existing = (await _reportNewspaperRepo.GetByReportIdAsync(reportId))
                           .Select(rn => rn.NewspaperId)
                           .ToHashSet();

            return filtered.Select(n => new ReportNewspaperPickerDTO
            {
                NewspaperId = n.Id,
                Date = n.Date,
                Title = n.Title,
                PublicationId = n.PublicationId,
                PRValue = n.PRValue,
                ADValue = n.ADValue,
                ArticleBranding = n.ArticleBranding,
                HeadlineBranding = n.HeadlineBranding,
                Toning = n.Toning,
                Selected = existing.Contains(n.Id)
            }).ToList();
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
        /// Async mapper: resolves PR/AD values from ClientArticle and ClientNewsPaper
        /// using the report's CustomerId as the join key.
        /// </summary>
        private async Task<ReportDetailsDTO> MapToDetailsDTOAsync(Report r)
        {
            // ── Pull client-specific records once — keyed by source id ──────────
            var clientArticles = (await _clientArticleRepo.GetByClientIdAsync(r.CustomerId))
                                  .ToDictionary(ca => ca.ArticleId);

            var clientNewsPapers = (await _clientNewsPaperRepo.GetByClientIdAsync(r.CustomerId))
                                    .ToDictionary(cn => cn.NewsPaperId);

            // ── Publication name lookup — NewsPaper has no nav property ──────────
            var publications = (await _publicationRepo.GetActivePublicationsAsync())
                                .ToDictionary(p => p.Id, p => p.PublicationName);

            // ── Build DTO ────────────────────────────────────────────────────────
            var dto = new ReportDetailsDTO
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer?.Name,
                ReportName = r.ReportName,
                ReportDate = r.ReportDate,
                Publish = r.Publish,
                ReportType = r.ReportType
            };

            // Articles — PR/AD come from ClientArticle (ArticleId + CustomerId)
            dto.Articles = r.ReportArticles.Select(ra =>
            {
                clientArticles.TryGetValue(ra.ArticleId, out var ca);

                return new ReportArticlePickerDTO
                {
                    ArticleId = ra.ArticleId,
                    Date = ra.Article?.Date ?? default,
                    Title = ra.Article?.Title,
                    ArticleURL = ra.Article?.ArticleURL,
                    WebsiteName = ra.Article?.Website?.WebsiteName,
                    WriterName = ra.Article?.Writer?.WriterName,
                    Language = ra.Article?.Language,
                    ArticleBranding = ra.Article?.ArticleBranding,
                    PRValue = ca?.PRValue ?? 0m,
                    ADValue = ca?.ADValue ?? 0m,
                    Selected = true
                };
            }).ToList();

            // Newspapers — PR/AD come from ClientNewsPaper (NewsPaperId + ClientId)
            dto.Newspapers = r.ReportNewspapers.Select(rn =>
            {
                clientNewsPapers.TryGetValue(rn.NewspaperId, out var cn);

                return new ReportNewspaperPickerDTO
                {
                    NewspaperId = rn.NewspaperId,
                    Date = rn.NewsPaper?.Date ?? default,
                    Title = cn?.Title ?? rn.NewsPaper?.Title,
                    PublicationId = rn.NewsPaper?.PublicationId ?? 0,
                    PublicationName = rn.NewsPaper != null && publications.TryGetValue(rn.NewsPaper.PublicationId, out var pubName) ? pubName : null,
                    PRValue = cn?.PRValue ?? 0m,
                    ADValue = cn?.ADValue ?? 0m,
                    ArticleBranding = cn?.ArticleBranding ?? rn.NewsPaper?.ArticleBranding,
                    HeadlineBranding = cn?.HeadlineBranding ?? rn.NewsPaper?.HeadlineBranding,
                    Toning = cn?.Toning ?? rn.NewsPaper?.Toning,
                    Selected = true
                };
            }).ToList();

            dto.ArticleCount = dto.Articles.Count;
            dto.NewspaperCount = dto.Newspapers.Count;

            return dto;
        }
    }
}
