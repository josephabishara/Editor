using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.MediaViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // ── List — roots only ──────────────────────────────────────────────────

        public async Task<ClientNewsPaperListDTO> GetListAsync(int clientId)
        {
            var client = await _clientRepo.GetByIdAsync(clientId);
            var items = await _clientNewsPaperRepo.GetByClientIdAsync(clientId);
            return new ClientNewsPaperListDTO
            {
                ClientId = clientId,
                ClientName = client?.Name ?? string.Empty,
                Items = items
                             .Where(n => n.ParentId == null)   // roots only
                             .Select(MapToDTO)
                             .ToList()
            };
        }

        public async Task<ClientNewsPaperDTO?> GetByIdAsync(int id)
        {
            var entity = await _clientNewsPaperRepo.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;

            var dto = MapToDTO(entity);

            // Load children
            var children = await _clientNewsPaperRepo.GetChildrenAsync(id);
            dto.Children = children.Select(MapToChildDTO).ToList();

            return dto;
        }

        // ── Create ─────────────────────────────────────────────────────────────
        // Flow:
        //   1. Insert NewsPaper master
        //   2. Insert parent ClientNewsPaper   (ParentId = null)
        //   3. For each child:
        //        a. Insert child NewsPaper master
        //        b. Insert child ClientNewsPaper (ParentId = parent.Id)

        public async Task<(bool Success, string Message, int NewId)> CreateAsync(ClientNewsPaperDTO model)
        {
            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = (model.Circulation ?? 0) * 4;

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
            await _newsPaperRepo.AddAsync(newsPaper);

            var clientNewsPaper = new ClientNewsPaper
            {
                NewsPaperId = newsPaper.Id,
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
                Publish = false,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _clientNewsPaperRepo.AddAsync(clientNewsPaper);

            return (true, "Newspaper article created successfully.", clientNewsPaper.Id);  // ← NewId added
        }

        // ── Update ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> UpdateAsync(ClientNewsPaperDTO model)
        {
            var existing = await _clientNewsPaperRepo.GetByIdWithDetailsAsync(model.Id);
            if (existing == null) return (false, "Record not found.");

            model.PRValue = Math.Round(model.ADValue * 3.5m, 2);
            model.Reach = (model.Circulation ?? 0) * 4;

            ApplyToClientNewsPaper(existing, model);
            existing.UpdatedAt = DateTime.UtcNow;
            await _clientNewsPaperRepo.UpdateAsync(existing);

            // Update NewsPaper master
            if (existing.NewsPaper != null)
            {
                ApplyToNewsPaper(existing.NewsPaper, model);
                existing.NewsPaper.UpdatedAt = DateTime.UtcNow;
                await _newsPaperRepo.UpdateAsync(existing.NewsPaper);
            }

            // ── Sync children ──────────────────────────────────────────────────
            var existingChildren = await _clientNewsPaperRepo.GetChildrenAsync(model.Id);

            foreach (var childDto in model.Children)
            {
                childDto.PRValue = Math.Round(childDto.ADValue * 3.5m, 2);
                childDto.Reach = (childDto.Circulation ?? 0) * 4;

                if (childDto.Id > 0)
                {
                    // Update existing child
                    var existingChild = existingChildren.FirstOrDefault(c => c.Id == childDto.Id);
                    if (existingChild != null)
                    {
                        ApplyChildToClientNewsPaper(existingChild, childDto, model);
                        existingChild.UpdatedAt = DateTime.UtcNow;
                        await _clientNewsPaperRepo.UpdateAsync(existingChild);

                        var childMaster = await _newsPaperRepo.GetByIdAsync(existingChild.NewsPaperId);
                        if (childMaster != null)
                        {
                            ApplyChildToNewsPaper(childMaster, childDto, model);
                            childMaster.UpdatedAt = DateTime.UtcNow;
                            await _newsPaperRepo.UpdateAsync(childMaster);
                        }
                    }
                }
                else
                {
                    // New child added on Edit
                    var childMaster = BuildNewsPaperFromChild(childDto, model);
                    await _newsPaperRepo.AddAsync(childMaster);

                    var childRecord = BuildClientNewsPaperFromChild(
                        childDto, model, childMaster.Id, parentId: model.Id);
                    await _clientNewsPaperRepo.AddAsync(childRecord);
                }
            }

            // Soft-delete removed children
            var submittedChildIds = model.Children
                .Where(c => c.Id > 0)
                .Select(c => c.Id)
                .ToHashSet();

            foreach (var orphan in existingChildren.Where(c => !submittedChildIds.Contains(c.Id)))
            {
                orphan.IsActive = false;
                orphan.Deleted = 1;
                orphan.DeletedAt = DateTime.UtcNow;
                await _clientNewsPaperRepo.UpdateAsync(orphan);
            }

            return (true, "Newspaper article updated successfully.");
        }

        // ── Delete (soft) ──────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _clientNewsPaperRepo.GetByIdWithDetailsAsync(id);
            if (existing == null) return (false, "Record not found.");

            // Soft-delete children first
            var children = await _clientNewsPaperRepo.GetChildrenAsync(id);
            foreach (var child in children)
            {
                child.IsActive = false;
                child.Deleted = 1;
                child.DeletedAt = DateTime.UtcNow;
                await _clientNewsPaperRepo.UpdateAsync(child);
            }

            existing.IsActive = false;
            existing.Deleted = 1;
            existing.DeletedAt = DateTime.UtcNow;
            await _clientNewsPaperRepo.UpdateAsync(existing);

            // Soft-delete master only if no other clients reference it
            var others = await _clientNewsPaperRepo.GetByNewsPaperIdAsync(existing.NewsPaperId);
            if (!others.Any(o => o.Id != id && o.IsActive))
            {
                existing.NewsPaper.IsActive = false;
                existing.NewsPaper.Deleted = 1;
                existing.NewsPaper.DeletedAt = DateTime.UtcNow;
                await _newsPaperRepo.UpdateAsync(existing.NewsPaper);
            }

            return (true, "Record deleted successfully.");
        }

        public async Task<(bool Success, string Message)> BulkDeleteAsync(IEnumerable<int> ids)
        {
            var idList = ids?.Distinct().ToList() ?? new List<int>();
            if (!idList.Any()) return (false, "No records selected.");

            int deleted = 0;
            foreach (var id in idList)
            {
                var (success, _) = await DeleteAsync(id);   // reuses the master-cascade check already in DeleteAsync
                if (success) deleted++;
            }

            return deleted == idList.Count
                ? (true, $"{deleted} newspaper(s) deleted successfully.")
                : (false, $"{deleted} of {idList.Count} newspaper(s) deleted — some records were not found.");
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

        // ═══════════════════════════════════════════════════════════════════════
        // PRIVATE BUILDERS
        // ═══════════════════════════════════════════════════════════════════════

        private static NewsPaper BuildNewsPaper(ClientNewsPaperDTO m) => new()
        {
            PublicationId = m.PublicationId,
            Date = m.Date,
            Title = m.Title,
            ADValue = m.ADValue,
            PRValue = m.PRValue,
            ArticleBranding = m.ArticleBranding,
            HeadlineBranding = m.HeadlineBranding,
            Toning = m.Toning,
            pictureInArticle = m.PictureinArticle == "Yes",
            Generation = m.Generation == "Generated",
            Content = m.Content,
            Images = m.Images,       // ← images saved here too
            IsActive = true,
            Deleted = 0,
            CreatedAt = DateTime.UtcNow
        };

        private static NewsPaper BuildNewsPaperFromChild(
            ChildNewsPaperDTO child, ClientNewsPaperDTO parent) => new()
            {
                PublicationId = child.PublicationId,
                Date = child.Date,
                Title = parent.Title,         // ← inherited
                ADValue = child.ADValue,
                PRValue = child.PRValue,
                ArticleBranding = parent.ArticleBranding,
                HeadlineBranding = parent.HeadlineBranding,
                Toning = parent.Toning,
                Content = parent.Content,       // ← inherited
                Images = parent.Images,        // ← inherited
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };

        private static ClientNewsPaper BuildClientNewsPaper(
            ClientNewsPaperDTO m, int newsPaperId, int? parentId) => new()
            {
                NewsPaperId = newsPaperId,
                ClientId = m.ClientId,
                ParentId = parentId,
                PublicationId = m.PublicationId,
                CategoryId = m.CategoryId,
                SubCategoryId = m.SubCategoryId,
                WriterId = m.WriterId,
                Date = m.Date,
                Title = m.Title,
                Pages = m.Pages,
                Height = m.Height,
                Width = m.Width,
                ADValue = m.ADValue,
                PRValue = m.PRValue,
                ArticleBranding = m.ArticleBranding,
                HeadlineBranding = m.HeadlineBranding,
                Toning = m.Toning,
                Content = m.Content,
                Images = m.Images,
                MediaType = m.MediaType,
                MediaTier = m.MediaTier,
                Frequency = m.Frequency,
                Language = m.Language,
                Circulation = m.Circulation,
                Reach = m.Reach,
                Publish = false,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };

        private static ClientNewsPaper BuildClientNewsPaperFromChild(
            ChildNewsPaperDTO child, ClientNewsPaperDTO parent,
            int newsPaperId, int parentId) => new()
            {
                NewsPaperId = newsPaperId,
                ClientId = parent.ClientId,
                ParentId = parentId,              // ← links to parent row
                PublicationId = child.PublicationId,   // ← child-specific
                CategoryId = parent.CategoryId,     // ← inherited
                SubCategoryId = parent.SubCategoryId,  // ← inherited
                WriterId = child.WriterId,         // ← child-specific
                Date = child.Date,             // ← child-specific
                Title = parent.Title,           // ← inherited
                Pages = child.Pages,            // ← child-specific
                Height = child.Height,           // ← child-specific
                Width = child.Width,            // ← child-specific
                ADValue = child.ADValue,
                PRValue = child.PRValue,
                ArticleBranding = parent.ArticleBranding,
                HeadlineBranding = parent.HeadlineBranding,
                Toning = parent.Toning,
                Content = parent.Content,         // ← inherited
                Images = parent.Images,          // ← inherited
                MediaType = child.MediaType,
                MediaTier = child.MediaTier,
                Frequency = child.Frequency,
                Language = child.Language ?? parent.Language,
                Circulation = child.Circulation,
                Reach = child.Reach,
                Publish = false,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };

        // ── Apply update helpers ───────────────────────────────────────────────

        private static void ApplyToClientNewsPaper(ClientNewsPaper e, ClientNewsPaperDTO m)
        {
            e.PublicationId = m.PublicationId;
            e.CategoryId = m.CategoryId;
            e.SubCategoryId = m.SubCategoryId;
            e.WriterId = m.WriterId;
            e.Date = m.Date;
            e.Title = m.Title;
            e.Pages = m.Pages;
            e.Height = m.Height;
            e.Width = m.Width;
            e.ADValue = m.ADValue;
            e.PRValue = m.PRValue;
            e.ArticleBranding = m.ArticleBranding;
            e.HeadlineBranding = m.HeadlineBranding;
      
            e.Toning = m.Toning;
            e.Content = m.Content;
            e.Images = m.Images;
            e.MediaType = m.MediaType;
            e.MediaTier = m.MediaTier;
            e.Frequency = m.Frequency;
            e.Language = m.Language;
            e.Circulation = m.Circulation;
            e.Reach = m.Reach;
        }

        private static void ApplyToNewsPaper(NewsPaper g, ClientNewsPaperDTO m)
        {
            g.Title = m.Title;
            g.ADValue = m.ADValue;
            g.PRValue = m.PRValue;
            g.ArticleBranding = m.ArticleBranding;
            g.HeadlineBranding = m.HeadlineBranding;
            g.pictureInArticle = m.PictureinArticle == "Yes";
            g.Generation = m.Generation == "Generated";
            g.Toning = m.Toning;
            g.Content = m.Content;
            g.Images = m.Images;
        }

        private static void ApplyChildToClientNewsPaper(
            ClientNewsPaper e, ChildNewsPaperDTO child, ClientNewsPaperDTO parent)
        {
            e.PublicationId = child.PublicationId;
            e.WriterId = child.WriterId;
            e.Date = child.Date;
            e.Pages = child.Pages;
            e.Height = child.Height;
            e.Width = child.Width;
            e.ADValue = child.ADValue;
            e.PRValue = child.PRValue;
            e.MediaType = child.MediaType;
            e.MediaTier = child.MediaTier;
            e.Frequency = child.Frequency;
            e.Language = child.Language ?? parent.Language;
            e.Circulation = child.Circulation;
            e.Reach = child.Reach;
            // inherited from parent:
            e.Title = parent.Title;
            e.Content = parent.Content;
            e.Images = parent.Images;
            e.CategoryId = parent.CategoryId;
            e.SubCategoryId = parent.SubCategoryId;
            e.ArticleBranding = parent.ArticleBranding;
            e.HeadlineBranding = parent.HeadlineBranding;
            e.Toning = parent.Toning;
        }

        private static void ApplyChildToNewsPaper(
            NewsPaper g, ChildNewsPaperDTO child, ClientNewsPaperDTO parent)
        {
            g.PublicationId = child.PublicationId;
            g.Date = child.Date;
            g.ADValue = child.ADValue;
            g.PRValue = child.PRValue;
            g.Title = parent.Title;
            g.Content = parent.Content;
            g.Images = parent.Images;
            g.ArticleBranding = parent.ArticleBranding;
            g.HeadlineBranding = parent.HeadlineBranding;
            g.Toning = parent.Toning;
        }

        // ── Mappers ────────────────────────────────────────────────────────────

        private static ClientNewsPaperDTO MapToDTO(ClientNewsPaper e) => new()
        {
            Id = e.Id,
            NewsPaperId = e.NewsPaperId,
            ClientId = e.ClientId,
            ClientName = e.Client?.Name,
            ParentId = e.ParentId,
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
            ADValue = e.ADValue ?? 0,
            PRValue = e.PRValue ?? 0,
            ArticleBranding = e.ArticleBranding,
            HeadlineBranding = e.HeadlineBranding,
            PictureinArticle = e.pictureInArticle == true ? "Yes" : "No",
            Generation = e.Generation == true ? "Generated" : "Not Generated",
            Toning = e.Toning,
            Content = e.Content,
            Images = e.Images,
            MediaType = e.MediaType,
            MediaTier = e.MediaTier,
            Frequency = e.Frequency,
            Language = e.Language,
            Circulation = e.Circulation,
            Reach = e.Reach,
            Publish = e.Publish
        };

        private static ChildNewsPaperDTO MapToChildDTO(ClientNewsPaper e) => new()
        {
            Id = e.Id,
            PublicationId = e.PublicationId,
            WriterId = e.WriterId,
            Date = e.Date,
            Pages = e.Pages,
            Height = e.Height,
            Width = e.Width,
            ADValue = e.ADValue ?? 0,
            PRValue = e.PRValue ?? 0,
            MediaType = e.MediaType,
            MediaTier = e.MediaTier,
            Frequency = e.Frequency,
            Language = e.Language,
            Circulation = e.Circulation,
            Reach = e.Reach
        };


        public async Task<(bool Success, string Message, int NewId)> DuplicateAsync(int id)
        {
            var original = await _clientNewsPaperRepo.GetByIdWithDetailsAsync(id);
            if (original == null) return (false, "Newspaper not found.", 0);

            // ── Step 1: Duplicate NewsPaper master ────────────────────────────────────
            var newMaster = new EditorEntitiesLayer.Entities.NewsPaper
            {
                PublicationId = original.PublicationId,
                Date = original.Date,
                Title = original.Title + " (Copy)",
                ADValue = original.ADValue ?? 0,
                PRValue = original.PRValue ?? 0,
                ArticleBranding = original.ArticleBranding,
                HeadlineBranding = original.HeadlineBranding,
                Toning = original.Toning,
                Content = original.Content,
                Images = original.Images,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _newsPaperRepo.AddAsync(newMaster);

            // ── Step 2: Duplicate parent ClientNewsPaper ──────────────────────────────
            var newParent = new EditorEntitiesLayer.Entities.ClientNewsPaper
            {
                NewsPaperId = newMaster.Id,
                ClientId = original.ClientId,
                ParentId = null,
                PublicationId = original.PublicationId,
                CategoryId = original.CategoryId,
                SubCategoryId = original.SubCategoryId,
                WriterId = original.WriterId,
                Date = original.Date,
                Title = original.Title + " (Copy)",
                Pages = original.Pages,
                Height = original.Height,
                Width = original.Width,
                ADValue = original.ADValue,
                PRValue = original.PRValue,
                ArticleBranding = original.ArticleBranding,
                HeadlineBranding = original.HeadlineBranding,
                Toning = original.Toning,
                Content = original.Content,
                Images = original.Images,
                MediaType = original.MediaType,
                MediaTier = original.MediaTier,
                Frequency = original.Frequency,
                Language = original.Language,
                Circulation = original.Circulation,
                Reach = original.Reach,
                Publish = false,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _clientNewsPaperRepo.AddAsync(newParent);

            // ── Step 3: Duplicate each child ──────────────────────────────────────────
            var children = await _clientNewsPaperRepo.GetChildrenAsync(id);
            foreach (var child in children)
            {
                var childMaster = new EditorEntitiesLayer.Entities.NewsPaper
                {
                    PublicationId = child.PublicationId,
                    Date = child.Date,
                    Title = newParent.Title,
                    ADValue = child.ADValue ?? 0,
                    PRValue = child.PRValue ?? 0,
                    ArticleBranding = child.ArticleBranding,
                    HeadlineBranding = child.HeadlineBranding,
                    Toning = child.Toning,
                    Content = child.Content,
                    Images = child.Images,
                    IsActive = true,
                    Deleted = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _newsPaperRepo.AddAsync(childMaster);

                var newChild = new EditorEntitiesLayer.Entities.ClientNewsPaper
                {
                    NewsPaperId = childMaster.Id,
                    ClientId = child.ClientId,
                    ParentId = newParent.Id,
                    PublicationId = child.PublicationId,
                    CategoryId = child.CategoryId,
                    SubCategoryId = child.SubCategoryId,
                    WriterId = child.WriterId,
                    Date = child.Date,
                    Title = child.Title,
                    Pages = child.Pages,
                    Height = child.Height,
                    Width = child.Width,
                    ADValue = child.ADValue,
                    PRValue = child.PRValue,
                    ArticleBranding = child.ArticleBranding,
                    HeadlineBranding = child.HeadlineBranding,
                    Toning = child.Toning,
                    Content = child.Content,
                    Images = child.Images,
                    MediaType = child.MediaType,
                    MediaTier = child.MediaTier,
                    Frequency = child.Frequency,
                    Language = child.Language,
                    Circulation = child.Circulation,
                    Reach = child.Reach,
                    Publish = false,
                    IsActive = true,
                    Deleted = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _clientNewsPaperRepo.AddAsync(newChild);
            }

            return (true, "Newspaper duplicated successfully.", newParent.Id);
        }

    }
}
