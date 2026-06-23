using ClosedXML.Excel;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.GeneralArticleViewModel;
using EditorViewModelLayer.MediaViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.GeneralArticle
{
    public class GeneralArticleService : IGeneralArticleService
    {
        private readonly IGeneralArticleRepository _repo;
        private readonly IWebsiteRepository _websiteRepo;
        private readonly IWriterRepository _writerRepo;

        public GeneralArticleService(
            IGeneralArticleRepository repo,
            IWebsiteRepository websiteRepo,
            IWriterRepository writerRepo)
        {
            _repo = repo;
            _websiteRepo = websiteRepo;
            _writerRepo = writerRepo;
        }

        // ── CRUD ───────────────────────────────────────────────────────────────

        public async Task<IEnumerable<GeneralArticleDTO>> GetAllAsync()
        {
            var list = await _repo.GetActiveAsync();
            return list.Select(MapToDTO);
        }

        public async Task<GeneralArticleDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdWithNavAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<(bool Success, string Message)> CreateAsync(GeneralArticleDTO model)
        {
            var entity = MapToEntity(model);
            entity.IsActive = true;
            entity.Deleted = 0;
            entity.CreatedAt = DateTime.UtcNow;

            await _repo.AddAsync(entity);
            return (true, "Article created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(GeneralArticleDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Article not found.");

            existing.Date = model.Date;
            existing.WebsiteId = model.WebsiteId;
            existing.WriterId = model.WriterId;
            existing.Language = model.Language;
            existing.ArticleBranding = model.ArticleBranding;
            existing.HeadlineBranding = model.HeadlineBranding;
            existing.PictureinArticle = model.PictureinArticle;
            existing.Generation = model.Generation;
            existing.ArticleURL = model.ArticleURL;
            existing.Title = model.Title;
            existing.Content = model.Content;
            existing.Images = model.Images;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Article updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Article not found.");

            existing.Deleted = 1;
            existing.IsActive = false;
            existing.DeletedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Article deleted successfully.");
        }

        // ── Filtering (Index) ───────────────────────────────────────────────────

        public async Task<IEnumerable<GeneralArticleDTO>> GetFilteredAsync(GeneralArticleFilterDTO filter)
        {
            var list = await _repo.GetFilteredAsync(
                filter.FromDate,
                filter.ToDate,
                filter.Title,
                filter.WebsiteId);

            return list.Select(MapToDTO);
        }

        public async Task<GeneralArticleIndexVM> GetIndexViewModelAsync(GeneralArticleFilterDTO filter)
        {
            // Sequential awaits — single DbContext, avoid Task.WhenAll concurrency issues
            var items = await GetFilteredAsync(filter);
            var websites = await _websiteRepo.GetActiveWebsitesAsync();

            return new GeneralArticleIndexVM
            {
                Items = items,
                Filter = filter,
                Websites = websites
                    .OrderBy(w => w.WebsiteName)
                    .Select(w => new MediaSelectOption
                    {
                        Value = w.Id.ToString(),
                        Text = w.WebsiteName,
                        Selected = filter.WebsiteId.HasValue && filter.WebsiteId.Value == w.Id
                    })
                    .ToList()
            };
        }

        // ── Export to Excel ────────────────────────────────────────────────────

        public byte[] ExportToExcel(IEnumerable<GeneralArticleDTO> articles)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("GeneralArticles");

            var headers = new[]
            {
                "Id", "Date", "Website", "Writer", "Language",
                "Article Branding", "Headline Branding", "Picture in Article",
                "AI Generated", "Article URL", "Title"
                // Content/Images excluded — too large for tabular export
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cell(1, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.White;
            }

            int row = 2;
            foreach (var a in articles)
            {
                ws.Cell(row, 1).Value = a.Id;
                ws.Cell(row, 2).Value = a.Date.ToString("yyyy-MM-dd");
                ws.Cell(row, 3).Value = a.WebsiteName ?? "";
                ws.Cell(row, 4).Value = a.WriterName ?? "";
                ws.Cell(row, 5).Value = a.Language ?? "";
                ws.Cell(row, 6).Value = a.ArticleBranding ?? "";
                ws.Cell(row, 7).Value = a.HeadlineBranding ?? "";
                ws.Cell(row, 8).Value = a.PictureinArticle ?? "";
                ws.Cell(row, 9).Value = a.Generation == true ? "Yes" : "No";
                ws.Cell(row, 10).Value = a.ArticleURL ?? "";
                ws.Cell(row, 11).Value = a.Title ?? "";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#EBF3FB");

                ws.Range(row, 1, row, headers.Length)
                  .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ── Import from Excel ──────────────────────────────────────────────────

        public async Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(
            Stream fileStream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return (false, "Only .xlsx and .xls files are supported.", 0);

            // Build lookup dictionaries for Website and Writer by name
            var websites = (await _websiteRepo.GetActiveWebsitesAsync())
                           .ToDictionary(w => w.WebsiteName.Trim().ToLower(), w => w.Id);
            var writers = (await _writerRepo.GetActiveWritersAsync())
                           .ToDictionary(w => w.WriterName.Trim().ToLower(), w => w.Id);

            var toImport = new List<EditorEntitiesLayer.Entities.GeneralArticle>();
            var errors = new List<string>();

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();

                if (rows == null || rows.Count == 0)
                    return (false, "The file has no data rows.", 0);

                int rowNum = 2;
                foreach (var row in rows)
                {
                    var dateStr = row.Cell(2).GetString().Trim();
                    var websiteStr = row.Cell(3).GetString().Trim().ToLower();
                    var writerStr = row.Cell(4).GetString().Trim().ToLower();
                    var title = row.Cell(11).GetString().Trim();

                    if (!DateTime.TryParse(dateStr, out var date))
                    {
                        errors.Add($"Row {rowNum}: Invalid date '{dateStr}'.");
                        rowNum++; continue;
                    }

                    if (string.IsNullOrWhiteSpace(title))
                    {
                        errors.Add($"Row {rowNum}: Title is required.");
                        rowNum++; continue;
                    }

                    if (!websites.TryGetValue(websiteStr, out var websiteId))
                    {
                        errors.Add($"Row {rowNum}: Website '{row.Cell(3).GetString()}' not found.");
                        rowNum++; continue;
                    }

                    if (!writers.TryGetValue(writerStr, out var writerId))
                    {
                        errors.Add($"Row {rowNum}: Writer '{row.Cell(4).GetString()}' not found.");
                        rowNum++; continue;
                    }

                    var genStr = row.Cell(9).GetString().Trim().ToLower();

                    toImport.Add(new EditorEntitiesLayer.Entities.GeneralArticle
                    {
                        Date = date,
                        WebsiteId = websiteId,
                        WriterId = writerId,
                        Language = row.Cell(5).GetString().Trim(),
                        ArticleBranding = row.Cell(6).GetString().Trim(),
                        HeadlineBranding = row.Cell(7).GetString().Trim(),
                        PictureinArticle = row.Cell(8).GetString().Trim(),
                        Generation = genStr == "yes" || genStr == "true",
                        ArticleURL = row.Cell(10).GetString().Trim(),
                        Title = title,
                        IsActive = true,
                        Deleted = 0,
                        CreatedAt = DateTime.UtcNow
                    });

                    rowNum++;
                }
            }
            catch (Exception ex)
            {
                return (false, $"Failed to read file: {ex.Message}", 0);
            }

            if (errors.Any())
                return (false, string.Join(" | ", errors), 0);

            foreach (var entity in toImport)
                await _repo.AddAsync(entity);

            return (true, $"{toImport.Count} article(s) imported successfully.", toImport.Count);
        }

        // ── Mappers ────────────────────────────────────────────────────────────

        private static GeneralArticleDTO MapToDTO(EditorEntitiesLayer.Entities.GeneralArticle a) => new()
        {
            Id = a.Id,
            Date = a.Date,
            WebsiteId = a.WebsiteId,
            WebsiteName = a.Website?.WebsiteName,
            WriterId = a.WriterId,
            WriterName = a.Writer?.WriterName,
            Language = a.Language,
            ArticleBranding = a.ArticleBranding,
            HeadlineBranding = a.HeadlineBranding,
            PictureinArticle = a.PictureinArticle,
            Generation = a.Generation ?? false,
            ArticleURL = a.ArticleURL,
            Title = a.Title,
            Content = a.Content,
            Images = a.Images
        };

        private static EditorEntitiesLayer.Entities.GeneralArticle MapToEntity(GeneralArticleDTO dto) => new()
        {
            Id = dto.Id,
            Date = dto.Date,
            WebsiteId = dto.WebsiteId,
            WriterId = dto.WriterId,
            Language = dto.Language,
            ArticleBranding = dto.ArticleBranding,
            HeadlineBranding = dto.HeadlineBranding,
            PictureinArticle = dto.PictureinArticle,
            Generation = dto.Generation,
            ArticleURL = dto.ArticleURL,
            Title = dto.Title,
            Content = dto.Content,
            Images = dto.Images
        };
    }
}
