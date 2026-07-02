using ClosedXML.Excel;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.GeneralNewspaperViewModel;
using EditorViewModelLayer.MediaViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.GeneralNewspaper
{
    public class GeneralNewspaperService : IGeneralNewspaperService
    {
        private readonly IGeneralNewspaperRepository _repo;
        private readonly IPublicationRepository _publicationRepo;
        private readonly IWriterRepository _writerRepo;

        public GeneralNewspaperService(
            IGeneralNewspaperRepository repo,
            IPublicationRepository publicationRepo,
            IWriterRepository writerRepo)
        {
            _repo = repo;
            _publicationRepo = publicationRepo;
            _writerRepo = writerRepo;
        }

        // ── CRUD ───────────────────────────────────────────────────────────────

        public async Task<IEnumerable<GeneralNewspaperDTO>> GetAllAsync()
        {
            var list = await _repo.GetActiveAsync();
            return list.Select(MapToDTO);
        }

        public async Task<GeneralNewspaperDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdWithNavAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<(bool Success, string Message)> CreateAsync(GeneralNewspaperDTO model)
        {
            var entity = MapToEntity(model);
            entity.IsActive = true;
            entity.Deleted = 0;
            entity.CreatedAt = DateTime.UtcNow;

            await _repo.AddAsync(entity);
            return (true, "Newspaper created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(GeneralNewspaperDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Newspaper not found.");

            existing.Date = model.Date;
            existing.PublicationId = model.PublicationId;
            existing.WriterId = model.WriterId;
            existing.Pages = model.Pages;
            existing.Height = model.Height;
            existing.Width = model.Width;
            existing.ArticleBranding = model.ArticleBranding;
            existing.HeadlineBranding = model.HeadlineBranding;
            existing.PictureinArticle = model.PictureinArticle;
            existing.Generation = model.Generation;
            existing.Toning = model.Toning;
            existing.Title = model.Title;
            existing.Content = model.Content;
            existing.Images = model.Images;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Newspaper updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Newspaper not found.");

            existing.Deleted = 1;
            existing.IsActive = false;
            existing.DeletedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Newspaper deleted successfully.");
        }

        // ── Filtering (Index) ───────────────────────────────────────────────────

        public async Task<IEnumerable<GeneralNewspaperDTO>> GetFilteredAsync(GeneralNewspaperFilterDTO filter)
        {
            var list = await _repo.GetFilteredAsync(
                filter.FromDate,
                filter.ToDate,
                filter.Title,
                filter.PublicationId);

            return list.Select(MapToDTO);
        }

        public async Task<GeneralNewspaperIndexVM> GetIndexViewModelAsync(GeneralNewspaperFilterDTO filter)
        {
            // Sequential awaits — single DbContext, avoid Task.WhenAll concurrency issues
            var items = await GetFilteredAsync(filter);
            var publications = await _publicationRepo.GetActivePublicationsAsync();

            return new GeneralNewspaperIndexVM
            {
                Items = items,
                Filter = filter,
                Publications = publications
                    .OrderBy(p => p.PublicationName)
                    .Select(p => new MediaSelectOption
                    {
                        Value = p.Id.ToString(),
                        Text = p.PublicationName,
                        Selected = filter.PublicationId.HasValue && filter.PublicationId.Value == p.Id
                    })
                    .ToList()
            };
        }

        // ── Export to Excel ────────────────────────────────────────────────────

        public byte[] ExportToExcel(IEnumerable<GeneralNewspaperDTO> newspapers)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("GeneralNewspapers");

            var headers = new[]
            {
                "Id", "Date", "Publication", "Writer", "Pages", "Height (cm)", "Width (cm)",
                "Article Branding", "Headline Branding", "Picture in Article",
                "AI Generated", "Toning", "Title"
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
            foreach (var a in newspapers)
            {
                ws.Cell(row, 1).Value = a.Id;
                ws.Cell(row, 2).Value = a.Date.ToString("yyyy-MM-dd");
                ws.Cell(row, 3).Value = a.PublicationName ?? "";
                ws.Cell(row, 4).Value = a.WriterName ?? "";
                ws.Cell(row, 5).Value = a.Pages;
                ws.Cell(row, 6).Value = a.Height;
                ws.Cell(row, 7).Value = a.Width;
                ws.Cell(row, 8).Value = a.ArticleBranding ?? "";
                ws.Cell(row, 9).Value = a.HeadlineBranding ?? "";
                ws.Cell(row, 10).Value = a.PictureinArticle ?? "";
                ws.Cell(row, 11).Value = a.Generation == true ? "Yes" : "No";
                ws.Cell(row, 12).Value = a.Toning ?? "";
                ws.Cell(row, 13).Value = a.Title ?? "";

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
        // Column layout matches ExportToExcel (minus Id, which is ignored on import):
        // 2 Date | 3 Publication | 4 Writer | 5 Pages | 6 Height | 7 Width |
        // 8 ArticleBranding | 9 HeadlineBranding | 10 PictureinArticle | 11 AI Generated | 12 Toning | 13 Title

        public async Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(
            Stream fileStream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return (false, "Only .xlsx and .xls files are supported.", 0);

            // Build lookup dictionaries for Publication and Writer by name
            var publications = (await _publicationRepo.GetActivePublicationsAsync())
                               .ToDictionary(p => p.PublicationName.Trim().ToLower(), p => p.Id);
            var writers = (await _writerRepo.GetActiveWritersAsync())
                           .ToDictionary(w => w.WriterName.Trim().ToLower(), w => w.Id);

            var toImport = new List<EditorEntitiesLayer.Entities.GeneralNewspaper>();
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
                    var pubStr = row.Cell(3).GetString().Trim().ToLower();
                    var writerStr = row.Cell(4).GetString().Trim().ToLower();
                    var title = row.Cell(13).GetString().Trim();

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

                    if (!publications.TryGetValue(pubStr, out var publicationId))
                    {
                        errors.Add($"Row {rowNum}: Publication '{row.Cell(3).GetString()}' not found.");
                        rowNum++; continue;
                    }

                    if (!writers.TryGetValue(writerStr, out var writerId))
                    {
                        errors.Add($"Row {rowNum}: Writer '{row.Cell(4).GetString()}' not found.");
                        rowNum++; continue;
                    }

                    var genStr = row.Cell(11).GetString().Trim().ToLower();

                    toImport.Add(new EditorEntitiesLayer.Entities.GeneralNewspaper
                    {
                        Date = date,
                        PublicationId = publicationId,
                        WriterId = writerId,
                        Pages = int.TryParse(row.Cell(5).GetString().Trim(), out var pages) ? pages : 0,
                        Height = decimal.TryParse(row.Cell(6).GetString().Trim(), out var h) ? h : 0,
                        Width = decimal.TryParse(row.Cell(7).GetString().Trim(), out var w) ? w : 0,
                        ArticleBranding = row.Cell(8).GetString().Trim(),
                        HeadlineBranding = row.Cell(9).GetString().Trim(),
                        PictureinArticle = row.Cell(10).GetString().Trim(),
                        Generation = genStr == "yes" || genStr == "true",
                        Toning = row.Cell(12).GetString().Trim(),
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

            return (true, $"{toImport.Count} newspaper(s) imported successfully.", toImport.Count);
        }

        // ── Dropdown builders (Create/Edit views) ───────────────────────────────

        public async Task<List<MediaSelectOption>> GetPublicationOptionsAsync(int selectedId = 0)
        {
            var publications = await _publicationRepo.GetActivePublicationsAsync();
            return publications
                .OrderBy(p => p.PublicationName)
                .Select(p => new MediaSelectOption
                {
                    Value = p.Id.ToString(),
                    Text = p.PublicationName,
                    Selected = p.Id == selectedId
                }).ToList();
        }

        public async Task<List<MediaSelectOption>> GetWriterOptionsAsync(int selectedId = 0)
        {
            var writers = await _writerRepo.GetActiveWritersAsync();
            return writers
                .OrderBy(w => w.WriterName)
                .Select(w => new MediaSelectOption
                {
                    Value = w.Id.ToString(),
                    Text = w.WriterName,
                    Selected = w.Id == selectedId
                }).ToList();
        }

        // ── Mappers ────────────────────────────────────────────────────────────

        private static GeneralNewspaperDTO MapToDTO(EditorEntitiesLayer.Entities.GeneralNewspaper a) => new()
        {
            Id = a.Id,
            Date = a.Date,
            PublicationId = a.PublicationId,
            PublicationName = a.Publication?.PublicationName,
            WriterId = a.WriterId,
            WriterName = a.Writer?.WriterName,
            Pages = a.Pages,
            Height = a.Height,
            Width = a.Width,
            ArticleBranding = a.ArticleBranding,
            HeadlineBranding = a.HeadlineBranding,
            PictureinArticle = a.PictureinArticle,
            Generation = a.Generation ?? false,
            Toning = a.Toning,
            Title = a.Title,
            Content = a.Content,
            Images = a.Images
        };

        private static EditorEntitiesLayer.Entities.GeneralNewspaper MapToEntity(GeneralNewspaperDTO dto) => new()
        {
            Id = dto.Id,
            Date = dto.Date,
            PublicationId = dto.PublicationId,
            WriterId = dto.WriterId,
            Pages = dto.Pages,
            Height = dto.Height,
            Width = dto.Width,
            ArticleBranding = dto.ArticleBranding,
            HeadlineBranding = dto.HeadlineBranding,
            PictureinArticle = dto.PictureinArticle,
            Generation = dto.Generation,
            Toning = dto.Toning,
            Title = dto.Title,
            Content = dto.Content,
            Images = dto.Images
        };
    }
}
