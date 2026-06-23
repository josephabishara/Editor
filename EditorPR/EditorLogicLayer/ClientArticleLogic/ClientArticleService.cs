using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.MediaViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            var rawItems = (await _clientArticleRepo.GetByClientIdAsync(clientId))
                                //.Where(a => a.ParentId == null)
                                .ToList();

            if (!rawItems.Any())
            {
                return new ClientArticleListDTO
                {
                    ClientId = clientId,
                    ClientName = client?.Name ?? string.Empty
                };
            }

            // ── One query per lookup table — no N+1 ──────────────────────────────────
            var categoryIds = rawItems.Select(a => a.CategoryId).Distinct().ToList();
            var subCategoryIds = rawItems.Select(a => a.SubCategoryId).Distinct().ToList();
            var websiteIds = rawItems.Select(a => a.WebsiteId).Distinct().ToList();
            var writerIds = rawItems.Select(a => a.WriterId).Distinct().ToList();
            var userIds = rawItems.Select(a => a.CreateId).Distinct().ToList();

            var categoryMap = await _context.Set<ClientCategories>()
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CategoryName);

            var subCategoryMap = await _context.Set<ClientCategories>()
                .Where(c => subCategoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.CategoryName);

            var websiteMap = await _context.Set<Websites>()
                .Where(w => websiteIds.Contains(w.Id))
                .ToDictionaryAsync(w => w.Id, w => w.WebsiteName);

            var writerMap = await _context.Set<EditorEntitiesLayer.Entities.Writer>()
                .Where(w => writerIds.Contains(w.Id))
                .ToDictionaryAsync(w => w.Id, w => w.WriterName);

            var userMap = await _context.Set<ApplicationUser>()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName ?? string.Empty);

            var dtos = rawItems
                .Select(a => MapToDTOWithLookups(
                    a, categoryMap, subCategoryMap, websiteMap, writerMap, userMap))
                .ToList();

            return new ClientArticleListDTO
            {
                ClientId = clientId,
                ClientName = client?.Name ?? string.Empty,
                Items = dtos
            };
        }



        public async Task<ClientArticleDTO?> GetByIdAsync(int id)
        {
            var e = await _clientArticleRepo.GetByIdWithDetailsAsync(id);
            if (e == null) return null;

            var dto = MapToDTO(e);

            // ── Resolve display names ─────────────────────────────────────────────────
            if (e.WebsiteId > 0)
            {
                var site = await _context.Set<Websites>().FindAsync(e.WebsiteId);
                dto.WebsiteName = site?.WebsiteName;
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

            if (e.WriterId > 0)
            {
                var writer = await _context.Set<EditorEntitiesLayer.Entities.Writer>().FindAsync(e.WriterId);
                dto.WriterName = writer?.WriterName;
            }

            if (e.CreateId > 0)
            {
                var user = await _context.Set<ApplicationUser>().FindAsync(e.CreateId);
                dto.CreatedByUserName = user?.FullName ?? user?.UserName;
            }

            // ── Load children ─────────────────────────────────────────────────────────
            var children = await _clientArticleRepo.GetChildrenAsync(id);
            dto.Children = children.Select(MapToChildDTO).ToList();

            return dto;
        }

        // ── Create ─────────────────────────────────────────────────────────────
        // Flow:
        //   1. Insert GeneralArticle master
        //   2. Insert parent ClientArticle  (ParentId = null)
        //   3. For each ChildArticleDTO in model.Children:
        //        a. Insert a child GeneralArticle  (inherits all except Website/Writer/Date/URL)
        //        b. Insert a child ClientArticle   (ParentId = parent.Id)

        public async Task<(bool Success, string Message, int NewId)> CreateAsync(ClientArticleDTO model)
        {
            // Auto-calculate for parent
            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = model.Impression * 4;


            // ── Step 1: GeneralArticle master ──────────────────────────────────
            var generalParent = BuildGeneralArticle(model);
            await _generalArticleRepo.AddAsync(generalParent);   // Id populated by EF

            // ── Step 2: Parent ClientArticle ───────────────────────────────────
            var parentArticle = BuildClientArticle(model, generalParent.Id, parentId: null);
            await _clientArticleRepo.AddAsync(parentArticle);    // Id populated by EF

            // ── Step 3: Children ───────────────────────────────────────────────
            foreach (var child in model.Children)
            {
                // Auto-calculate child values from its own Website
                child.PRValue = Math.Round(child.ADValue * 3.5m, 2);
                child.Reach = child.Impression * 4;

                // Child GeneralArticle — inherits content/images from parent,
                // overrides Website, Writer, Date, ArticleURL
                var generalChild = BuildGeneralArticleFromChild(child, model);
                await _generalArticleRepo.AddAsync(generalChild);

                // Child ClientArticle — linked to parent via ParentId
                var childArticle = BuildClientArticleFromChild(
                    child, model, generalChild.Id, parentId: parentArticle.Id);
                await _clientArticleRepo.AddAsync(childArticle);
            }

            var childCount = model.Children.Count;
            var msg = childCount > 0
                ? $"Article created with {childCount} child article(s)."
                : "Article created successfully.";

            return (true, msg, parentArticle.Id);
        }

        // ── Update ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> UpdateAsync(ClientArticleDTO model)
        {
            var existing = await _clientArticleRepo.GetByIdWithDetailsAsync(model.Id);
            if (existing == null) return (false, "Record not found.");

            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = model.Impression * 4;

            // Update parent ClientArticle
            ApplyToClientArticle(existing, model);
            existing.UpdatedAt = DateTime.UtcNow;
            await _clientArticleRepo.UpdateAsync(existing);

            // Update parent GeneralArticle master
            var general = await _generalArticleRepo.GetByIdAsync(existing.ArticleId);
            if (general != null)
            {
                ApplyToGeneralArticle(general, model);
                general.UpdatedAt = DateTime.UtcNow;
                await _generalArticleRepo.UpdateAsync(general);
            }

            // ── Sync children ──────────────────────────────────────────────────
            var existingChildren = await _clientArticleRepo.GetChildrenAsync(model.Id);

            foreach (var childDto in model.Children)
            {
                childDto.PRValue = Math.Round(childDto.ADValue * 3.5m, 2);
                childDto.Reach = childDto.Impression * 4;

                if (childDto.Id > 0)
                {
                    // Update existing child
                    var existingChild = existingChildren.FirstOrDefault(c => c.Id == childDto.Id);
                    if (existingChild != null)
                    {
                        ApplyChildToClientArticle(existingChild, childDto, model);
                        existingChild.UpdatedAt = DateTime.UtcNow;
                        await _clientArticleRepo.UpdateAsync(existingChild);

                        var childGeneral = await _generalArticleRepo.GetByIdAsync(existingChild.ArticleId);
                        if (childGeneral != null)
                        {
                            ApplyChildToGeneralArticle(childGeneral, childDto, model);
                            childGeneral.UpdatedAt = DateTime.UtcNow;
                            await _generalArticleRepo.UpdateAsync(childGeneral);
                        }
                    }
                }
                else
                {
                    // New child added during Edit
                    var generalChild = BuildGeneralArticleFromChild(childDto, model);
                    await _generalArticleRepo.AddAsync(generalChild);

                    var childArticle = BuildClientArticleFromChild(
                        childDto, model, generalChild.Id, parentId: model.Id);
                    await _clientArticleRepo.AddAsync(childArticle);
                }
            }

            // Soft-delete children removed from the form
            var submittedChildIds = model.Children
                .Where(c => c.Id > 0)
                .Select(c => c.Id)
                .ToHashSet();

            foreach (var orphan in existingChildren.Where(c => !submittedChildIds.Contains(c.Id)))
            {
                orphan.IsActive = false;
                orphan.Deleted = 1;
                orphan.DeletedAt = DateTime.UtcNow;
                await _clientArticleRepo.UpdateAsync(orphan);
            }

            return (true, "Article updated successfully.");
        }

        // ── Delete (soft) ──────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _clientArticleRepo.GetByIdWithDetailsAsync(id);
            if (existing == null) return (false, "Record not found.");

            // Soft-delete children first
            var children = await _clientArticleRepo.GetChildrenAsync(id);
            foreach (var child in children)
            {
                child.IsActive = false;
                child.Deleted = 1;
                child.DeletedAt = DateTime.UtcNow;
                await _clientArticleRepo.UpdateAsync(child);
            }

            existing.IsActive = false;
            existing.Deleted = 1;
            existing.DeletedAt = DateTime.UtcNow;
            await _clientArticleRepo.UpdateAsync(existing);

            return (true, "Record deleted.");
        }
        // ── Publish ────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> PublishAsync(int id)
        {
            var e = await _clientArticleRepo.GetByIdAsync(id);
            if (e == null) return (false, "Record not found.");
            e.Publish = true; e.UpdatedAt = DateTime.UtcNow;
            await _clientArticleRepo.UpdateAsync(e);
            return (true, "Published.");
        }

        public async Task<(bool Success, string Message)> UnpublishAsync(int id)
        {
            var e = await _clientArticleRepo.GetByIdAsync(id);
            if (e == null) return (false, "Record not found.");
            e.Publish = false; e.UpdatedAt = DateTime.UtcNow;
            await _clientArticleRepo.UpdateAsync(e);
            return (true, "Unpublished.");
        }
        // ── Dropdown builders (unchanged) ─────────────────────────────────────

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

        // ── AJAX auto-fill (unchanged) ────────────────────────────────────────

        public async Task<WebsiteAutoFillDTO> GetWebsiteAutoFillAsync(int websiteId, int clientId)
        {
            var site = await _websiteRepo.GetByIdAsync(websiteId);
            if (site == null) return new WebsiteAutoFillDTO();

            var wcc = await _context.Set<WebsiteCustomerCategory>()
                .FirstOrDefaultAsync(w => w.WebsiteId == websiteId && w.CustomerId == clientId);

            var adValue = wcc?.UnitPrice ?? 0;
            var impression = site.Impression;

            return new WebsiteAutoFillDTO
            {
                AdValue = adValue,
                PrValue = Math.Round(adValue * 3.5m, 2),
                MediaType = null,
                MediaTier = wcc?.MediaTier ?? site.MediaTier,
                Frequency = site.Frequency,
                Language = site.Language,
                Impression = impression,
                Reach = impression * 4
            };
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PRIVATE BUILDERS
        // ═══════════════════════════════════════════════════════════════════════

        // ── Build GeneralArticle from parent DTO ───────────────────────────────
        private static EditorEntitiesLayer.Entities.GeneralArticle BuildGeneralArticle(ClientArticleDTO m) => new()
        {
            Date = m.Date,
            WebsiteId = m.WebsiteId,
            WriterId = m.WriterId,
            Language = m.Language,
            ArticleBranding = m.ArticleBranding,
            HeadlineBranding = m.HeadlineBranding,
            PictureinArticle = m.PictureinArticle,
            Generation = m.Generation == "Generated",
            ArticleURL = m.ArticleURL,
            Title = m.Title,
            Content = m.Content,
            Images = m.Images,
            IsActive = true,
            Deleted = 0,
            CreatedAt = DateTime.UtcNow
        };

        // ── Build GeneralArticle for a child  ──────────────────────────────────
        // Inherits: Title, Content, Images, Branding, Toning, Generation
        // Overrides: Date, WebsiteId, WriterId, ArticleURL, Language (from child website)
        private static EditorEntitiesLayer.Entities.GeneralArticle BuildGeneralArticleFromChild(
            ChildArticleDTO child, ClientArticleDTO parent) => new()
            {
                Date = child.Date,
                WebsiteId = child.WebsiteId,
                WriterId = child.WriterId,
                Language = child.Language ?? parent.Language,
                ArticleBranding = parent.ArticleBranding,
                HeadlineBranding = parent.HeadlineBranding,
                PictureinArticle = parent.PictureinArticle,
                Generation = parent.Generation == "Generated",
                ArticleURL = child.ArticleURL,
                Title = parent.Title,     // ← inherited
                Content = parent.Content,   // ← inherited
                Images = parent.Images,    // ← inherited
                IsActive = true,
                Deleted = 0,

                CreatedAt = DateTime.UtcNow
            };

        // ── Build ClientArticle for parent ─────────────────────────────────────
        private static ClientArticle BuildClientArticle(
            ClientArticleDTO m, int generalId, int? parentId) => new()
            {
                ArticleId = generalId,
                ClientId = m.ClientId,
                ParentId = parentId,
                Date = m.Date,
                WebsiteId = m.WebsiteId,
                CategoryId = m.CategoryId,
                SubCategoryId = m.SubCategoryId,
                WriterId = m.WriterId,
                Frequency = m.Frequency,
                MediaType = m.MediaType,
                Impression = m.Impression,
                Reach = m.Reach,
                ADValue = m.ADValue,
                PRValue = m.PRValue,
                Toning = m.Toning,
                MediaTier = m.MediaTier,
                Language = m.Language,
                ArticleBranding = m.ArticleBranding,
                HeadlineBranding = m.HeadlineBranding,
                PictureinArticle = m.PictureinArticle,
                Generation = m.Generation == "Not Generated" ,
                ArticleURL = m.ArticleURL,
                Title = m.Title,
                Content = m.Content,
                Images = m.Images,
                IsActive = true,
                Deleted = 0,
                Publish = m.Publish,
                WebsiteType = m.WebsiteType,
                CreatedAt = DateTime.UtcNow
            };

        // ── Build ClientArticle for a child ────────────────────────────────────
        private static ClientArticle BuildClientArticleFromChild(
            ChildArticleDTO child, ClientArticleDTO parent, int generalId, int parentId) => new()
            {
                ArticleId = generalId,
                ClientId = parent.ClientId,
                ParentId = parentId,           // ← links to parent row
                Date = child.Date,         // ← child-specific
                WebsiteId = child.WebsiteId,    // ← child-specific
                WriterId = child.WriterId,     // ← child-specific
                CategoryId = parent.CategoryId,  // ← inherited
                SubCategoryId = parent.SubCategoryId,
                Frequency = child.Frequency,
                MediaType = child.MediaType,
                Impression = child.Impression,
                Reach = child.Reach,
                ADValue = child.ADValue,
                PRValue = child.PRValue,
                Toning = parent.Toning,      // ← inherited
                MediaTier = child.MediaTier,
                Language = child.Language ?? parent.Language,
                ArticleBranding = parent.ArticleBranding,
                HeadlineBranding = parent.HeadlineBranding,
                PictureinArticle = parent.PictureinArticle,
                Generation = parent.Generation == "Not Generated",
                ArticleURL = child.ArticleURL,   // ← child-specific
                Title = parent.Title,       // ← inherited
                Content = parent.Content,     // ← inherited
                Images = parent.Images,      // ← inherited
                IsActive = true,
                WebsiteType = parent.WebsiteType,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };

        // ── Apply update helpers ───────────────────────────────────────────────

        private static void ApplyToClientArticle(ClientArticle e, ClientArticleDTO m)
        {
            e.WebsiteId = m.WebsiteId;
            e.CategoryId = m.CategoryId;
            e.SubCategoryId = m.SubCategoryId;
            e.WriterId = m.WriterId;
            e.Publish = m.Publish;
            e.Date = m.Date;
            e.Frequency = m.Frequency;
            e.MediaType = m.MediaType;
            e.Impression = m.Impression;
            e.Reach = m.Reach;
            e.ADValue = m.ADValue;
            e.PRValue = m.PRValue;
            e.Toning = m.Toning;
            e.MediaTier = m.MediaTier;
            e.Language = m.Language;
            e.ArticleBranding = m.ArticleBranding;
            e.HeadlineBranding = m.HeadlineBranding;
            e.PictureinArticle = m.PictureinArticle;
            e.Generation = m.Generation == "Not Generated" ;
            e.ArticleURL = m.ArticleURL;
            e.Title = m.Title;
            e.Content = m.Content;
            e.Images = m.Images;
            e.WebsiteType = m.WebsiteType;
        }

        private static void ApplyToGeneralArticle(EditorEntitiesLayer.Entities.GeneralArticle g, ClientArticleDTO m)
        {
            g.Date = m.Date;
            g.WebsiteId = m.WebsiteId;
            g.WriterId = m.WriterId;
            g.Language = m.Language;
            g.ArticleBranding = m.ArticleBranding;
            g.HeadlineBranding = m.HeadlineBranding;
            g.PictureinArticle = m.PictureinArticle;
            g.Generation = m.Generation == "Not Generated";
            g.ArticleURL = m.ArticleURL;
            g.Title = m.Title;
            g.Content = m.Content;
            g.Images = m.Images;
        }

        private static void ApplyChildToClientArticle(
            ClientArticle e, ChildArticleDTO child, ClientArticleDTO parent)
        {
            e.Date = child.Date;
            e.WebsiteId = child.WebsiteId;
            e.WriterId = child.WriterId;
            e.ArticleURL = child.ArticleURL;
            e.Frequency = child.Frequency;
            e.MediaType = child.MediaType;
            e.Impression = child.Impression;
            e.Reach = child.Reach;
            e.ADValue = child.ADValue;
            e.PRValue = child.PRValue;
            e.MediaTier = child.MediaTier;
            e.Language = child.Language ?? parent.Language;
            // inherited from parent:
            e.Title = parent.Title;
            e.Content = parent.Content;
            e.Images = parent.Images;
            e.CategoryId = parent.CategoryId;
            e.SubCategoryId = parent.SubCategoryId;
            e.Toning = parent.Toning;
            e.ArticleBranding = parent.ArticleBranding;
            e.HeadlineBranding = parent.HeadlineBranding;
            e.PictureinArticle = parent.PictureinArticle;
            e.Generation = parent.Generation == "Not Generated";
            e.WebsiteType = parent.WebsiteType;
        }

        private static void ApplyChildToGeneralArticle(
            EditorEntitiesLayer.Entities.GeneralArticle g, ChildArticleDTO child, ClientArticleDTO parent)
        {
            g.Date = child.Date;
            g.WebsiteId = child.WebsiteId;
            g.WriterId = child.WriterId;
            g.ArticleURL = child.ArticleURL;
            g.Language = child.Language ?? parent.Language;
            g.Title = parent.Title;
            g.Content = parent.Content;
            g.Images = parent.Images;
            g.ArticleBranding = parent.ArticleBranding;
            g.HeadlineBranding = parent.HeadlineBranding;
            g.PictureinArticle = parent.PictureinArticle;
            g.Generation = parent.Generation == "Not Generated";

        }

        // ── Mappers ────────────────────────────────────────────────────────────

        private static ClientArticleDTO MapToDTO(ClientArticle e) => new()
        {
            Id = e.Id,
            ArticleId = e.ArticleId,
            ClientId = e.ClientId,
            ParentId = e.ParentId,
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
            Images = e.Images,
            Publish = e.Publish,
            CreatedAt = e.CreatedAt,
            WebsiteType = e.WebsiteType,
           // CreatedByUserName = e.CreatedByUserName ?? string.Empty,

        };

        /// <summary>Mapper with pre-fetched lookup dictionaries — used for list rendering.</summary>

        private static ClientArticleDTO MapToDTOWithLookups(
            ClientArticle e,
            Dictionary<int, string> categoryMap,
            Dictionary<int, string> subCategoryMap,
            Dictionary<int, string> websiteMap,
            Dictionary<int, string> writerMap,
            Dictionary<int, string> userMap) => new()
            {
                Id = e.Id,
                ArticleId = e.ArticleId,
                ClientId = e.ClientId,
                ParentId = e.ParentId,
                Date = e.Date,
                WebsiteId = e.WebsiteId,
                WebsiteName = websiteMap.GetValueOrDefault(e.WebsiteId),
                CategoryId = e.CategoryId,
                CategoryName = categoryMap.GetValueOrDefault(e.CategoryId),
                SubCategoryId = e.SubCategoryId,
                SubCategoryName = subCategoryMap.GetValueOrDefault(e.SubCategoryId),
                WriterId = e.WriterId,
                WriterName = writerMap.GetValueOrDefault(e.WriterId),
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
                Publish = e.Publish,
                Images = e.Images,
                CreatedAt = e.CreatedAt,
                WebsiteType = e.WebsiteType,
                CreatedByUserName = userMap.GetValueOrDefault(e.CreateId)
            };
        private static ChildArticleDTO MapToChildDTO(ClientArticle e) => new()
        {
            Id = e.Id,
            WebsiteId = e.WebsiteId,
            WriterId = e.WriterId,
            Date = e.Date,
            ArticleURL = e.ArticleURL,
            Frequency = e.Frequency,
            MediaType = e.MediaType,
            Impression = e.Impression,
            Reach = e.Reach,
            ADValue = e.ADValue ?? 0,
            PRValue = e.PRValue ?? 0,
            MediaTier = e.MediaTier,
            Language = e.Language
        };


        public async Task<(bool Success, string Message, int NewId)> DuplicateAsync(int id)
        {
            // ── Load original parent with its entity ──────────────────────────────────
            var original = await _clientArticleRepo.GetByIdWithDetailsAsync(id);
            if (original == null) return (false, "Article not found.", 0);

            // ── Step 1: Duplicate GeneralArticle master ───────────────────────────────
            var newGeneral = new EditorEntitiesLayer.Entities.GeneralArticle
            {
                Date = original.Date,
                WebsiteId = original.WebsiteId,
                WriterId = original.WriterId,
                Language = original.Language,
                ArticleBranding = original.ArticleBranding,
                HeadlineBranding = original.HeadlineBranding,
                PictureinArticle = original.PictureinArticle,
                Generation = original.Generation,
                ArticleURL = original.ArticleURL,
                Title = original.Title + " (Copy)",
                Content = original.Content,
                Images = original.Images,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _generalArticleRepo.AddAsync(newGeneral);

            // ── Step 2: Duplicate parent ClientArticle ────────────────────────────────
            var newParent = new ClientArticle
            {
                ArticleId = newGeneral.Id,
                ClientId = original.ClientId,
                ParentId = null,                      // always root
                Date = original.Date,
                WebsiteId = original.WebsiteId,
                CategoryId = original.CategoryId,
                SubCategoryId = original.SubCategoryId,
                WriterId = original.WriterId,
                Frequency = original.Frequency,
                MediaType = original.MediaType,
                Impression = original.Impression,
                Reach = original.Reach,
                ADValue = original.ADValue,
                PRValue = original.PRValue,
                Toning = original.Toning,
                MediaTier = original.MediaTier,
                Language = original.Language,
                ArticleBranding = original.ArticleBranding,
                HeadlineBranding = original.HeadlineBranding,
                PictureinArticle = original.PictureinArticle,
                Generation = original.Generation,
                ArticleURL = original.ArticleURL,
                Title = original.Title + " (Copy)",
                Content = original.Content,
                Images = original.Images,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _clientArticleRepo.AddAsync(newParent);

            // ── Step 3: Duplicate each child ──────────────────────────────────────────
            var children = await _clientArticleRepo.GetChildrenAsync(id);
            foreach (var child in children)
            {
                var childGeneral = new EditorEntitiesLayer.Entities.GeneralArticle
                {
                    Date = child.Date,
                    WebsiteId = child.WebsiteId,
                    WriterId = child.WriterId,
                    Language = child.Language,
                    ArticleBranding = child.ArticleBranding,
                    HeadlineBranding = child.HeadlineBranding,
                    PictureinArticle = child.PictureinArticle,
                    Generation = child.Generation,
                    ArticleURL = child.ArticleURL,
                    Title = newParent.Title,   // inherits updated title
                    Content = child.Content,
                    Images = child.Images,
                    IsActive = true,
                    Deleted = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _generalArticleRepo.AddAsync(childGeneral);

                var newChild = new ClientArticle
                {
                    ArticleId = childGeneral.Id,
                    ClientId = child.ClientId,
                    ParentId = newParent.Id,      // link to new parent
                    Date = child.Date,
                    WebsiteId = child.WebsiteId,
                    CategoryId = child.CategoryId,
                    SubCategoryId = child.SubCategoryId,
                    WriterId = child.WriterId,
                    Frequency = child.Frequency,
                    MediaType = child.MediaType,
                    Impression = child.Impression,
                    Reach = child.Reach,
                    ADValue = child.ADValue,
                    PRValue = child.PRValue,
                    Toning = child.Toning,
                    MediaTier = child.MediaTier,
                    Language = child.Language,
                    ArticleBranding = child.ArticleBranding,
                    HeadlineBranding = child.HeadlineBranding,
                    PictureinArticle = child.PictureinArticle,
                    Generation = child.Generation,
                    ArticleURL = child.ArticleURL,
                    Title = child.Title,
                    Content = child.Content,
                    Images = child.Images,
                    IsActive = true,
                    Deleted = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _clientArticleRepo.AddAsync(newChild);
            }

            return (true, $"Article duplicated successfully.", newParent.Id);
        }

        public async Task<(bool Success, string Message)> BulkDeleteAsync(IEnumerable<int> ids)
        {
            var idList = ids?.Distinct().ToList() ?? new List<int>();
            if (!idList.Any()) return (false, "No records selected.");

            int deleted = 0;
            foreach (var id in idList)
            {
                var (success, _) = await DeleteAsync(id);   // children cascade automatically — same as single delete
                if (success) deleted++;
            }

            return deleted == idList.Count
                ? (true, $"{deleted} article(s) deleted successfully.")
                : (false, $"{deleted} of {idList.Count} article(s) deleted — some records were not found.");
        }

    }
}