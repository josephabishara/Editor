using ClosedXML.Excel;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.PublicationViewModel;

namespace EditorLogicLayer.Publication
{
    public class PublicationService : IPublicationService
    {
        private readonly IPublicationRepository _repo;
        private readonly IClientRepository _clientRepo;
        private readonly IPublicationCustomerCategoryRepository _categoryRepo;

        public PublicationService(
           IPublicationRepository repo,
           IClientRepository clientRepo,
           IPublicationCustomerCategoryRepository categoryRepo)
        {
            _repo = repo;
            _clientRepo = clientRepo;
            _categoryRepo = categoryRepo;
        }
        public async Task<IEnumerable<PublicationDTO>> GetAllAsync()
        {
            var publications = await _repo.GetActivePublicationsAsync();
            return publications.Select(MapToDTO);
        }

        public async Task<PublicationDTO?> GetByIdAsync(int id)
        {
            var publication = await _repo.GetByIdAsync(id);
            return publication == null ? null : MapToDTO(publication);
        }

        public async Task<(bool Success, string Message)> CreateAsync(PublicationDTO model)
        {
            var entity = MapToEntity(model);
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.Deleted = 0;

            await _repo.AddAsync(entity);
            await FanOutToClientsAsync(entity);
            return (true, "Publication created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(PublicationDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null) return (false, "Publication not found.");

            existing.PublicationName = model.PublicationName;
            existing.MediaType = model.MediaType;
            existing.MediaTier = model.MediaTier;
            existing.Frequency = model.Frequency;
            existing.Distribution = model.Distribution;
            existing.Language = model.Language;
            existing.CmPrice = model.CmPrice;
            existing.Circulation = model.Circulation;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Publication updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return (false, "Publication not found.");

            existing.Deleted = 1;
            existing.IsActive = false;
            existing.DeletedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Publication deleted successfully.");
        }

        // ── Export ─────────────────────────────────────────────────────────────

        public byte[] ExportToExcel(IEnumerable<PublicationDTO> publications)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Publications");
            var headers = new[] { "Id", "Publication Name", "Media Type", "Media Tier", "Frequency", "Distribution", "Language", "CM Price", "Circulation" };

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
            foreach (var p in publications)
            {
                ws.Cell(row, 1).Value = p.Id;
                ws.Cell(row, 2).Value = p.PublicationName;
                ws.Cell(row, 3).Value = p.MediaType ?? "";
                ws.Cell(row, 4).Value = p.MediaTier ?? "";
                ws.Cell(row, 5).Value = p.Frequency ?? "";
                ws.Cell(row, 6).Value = p.Distribution ?? "";
                ws.Cell(row, 7).Value = p.Language ?? "";
                ws.Cell(row, 8).Value = p.CmPrice;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 9).Value = p.Circulation.HasValue ? p.Circulation.Value.ToString("N0") : "";
                if (row % 2 == 0) ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#EBF3FB");
                ws.Range(row, 1, row, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ── Import ─────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(
            Stream fileStream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls") return (false, "Only .xlsx and .xls files are supported.", 0);

            var toImport = new List<EditorEntitiesLayer.Entities.Publication>();
            var errors = new List<string>();

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
                if (rows == null || rows.Count == 0) return (false, "The file has no data rows.", 0);

                int rowNum = 2;
                foreach (var row in rows)
                {
                    var name = row.Cell(2).GetString().Trim();
                    //var url = row.Cell(3).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name)) { errors.Add($"Row {rowNum}: Publication Name is required."); rowNum++; continue; }
                   // if (string.IsNullOrWhiteSpace(url)) { errors.Add($"Row {rowNum}: URL is required."); rowNum++; continue; }

                    decimal.TryParse(row.Cell(10).GetString().Trim(), out decimal cmPrice);
                    int.TryParse(row.Cell(11).GetString().Trim().Replace(",", ""), out int circulation);

                    toImport.Add(new EditorEntitiesLayer.Entities.Publication
                    {
                        PublicationName = name,
                        //URL = url,
                        MediaType = row.Cell(3).GetString().Trim().NullIfEmpty(),
                        MediaTier = row.Cell(4).GetString().Trim().NullIfEmpty(),
                        Frequency = row.Cell(5).GetString().Trim().NullIfEmpty(),
                     //   Reach = row.Cell(7).GetString().Trim().NullIfEmpty(),
                        Distribution = row.Cell(6).GetString().Trim().NullIfEmpty(),
                        Language = row.Cell(7).GetString().Trim().NullIfEmpty(),
                        CmPrice = cmPrice,
                        Circulation = circulation > 0 ? circulation : null,
                        IsActive = true,
                        Deleted = 0,
                        CreatedAt = DateTime.UtcNow
                    });
                    rowNum++;
                }
            }
            catch (Exception ex) { return (false, $"Failed to read file: {ex.Message}", 0); }

            if (errors.Any()) return (false, string.Join(" | ", errors), 0);

            foreach (var entity in toImport)
            {
                await _repo.AddAsync(entity);
                await FanOutToClientsAsync(entity);
            }

            return (true, $"{toImport.Count} publication(s) imported successfully.", toImport.Count);
        }

        // ── Fan-out helper ─────────────────────────────────────────────────────

        private async Task FanOutToClientsAsync(EditorEntitiesLayer.Entities.Publication p)
        {
            var clients = await _clientRepo.GetActiveClientsAsync();
            if (!clients.Any()) return;

            var rows = clients.Select(c => new PublicationCustomerCategory
            {
                CustomerId = c.Id,
                PublicationId = p.Id,
                MediaType = p.MediaType,
                MediaTier = p.MediaTier,
                Frequency = p.Frequency,
                Distribution = p.Distribution,
                Language = p.Language,
                UnitPrice = p.CmPrice,   // source field is CmPrice
                Circulation = p.Circulation
            }).ToList();

            await _categoryRepo.AddRangeAsync(rows);
        }

        // ── Mappers ────────────────────────────────────────────────────────────

        private static PublicationDTO MapToDTO(EditorEntitiesLayer.Entities.Publication p) => new()
        {
            Id = p.Id,
            PublicationName = p.PublicationName,
            MediaType = p.MediaType,
            MediaTier = p.MediaTier,
            Frequency = p.Frequency,
            Distribution = p.Distribution,
            Language = p.Language,
            CmPrice = p.CmPrice,
            Circulation = p.Circulation
        };

        private static EditorEntitiesLayer.Entities.Publication MapToEntity(PublicationDTO dto) => new()
        {
            Id = dto.Id,
            PublicationName = dto.PublicationName,
            MediaType = dto.MediaType,
            MediaTier = dto.MediaTier,
            Frequency = dto.Frequency,
            Distribution = dto.Distribution,
            Language = dto.Language,
            CmPrice = dto.CmPrice,
            Circulation = dto.Circulation
        };
    }

    internal static class PubStringEx
    {
        public static string? NullIfEmpty(this string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
