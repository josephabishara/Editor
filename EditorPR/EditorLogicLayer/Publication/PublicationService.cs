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

        // ── Shared Excel column map — used by BOTH ExportToExcel and ImportFromExcelAsync.
        // Keeping a single source of truth here prevents the export/import drift that caused
        // CmPrice/Circulation (and later MediaType..Language) to be read from the wrong cells.
        private static class Col
        {
            public const int Id = 1;
            public const int Name = 2;
            public const int MediaType = 3;
            public const int MediaTier = 4;
            public const int Frequency = 5;
            public const int Distribution = 6;
            public const int Language = 7;
            public const int CmPrice = 8;
            public const int Circulation = 9;
            public const int Count = 9; // total columns — keep in sync with Headers below
        }

        private static readonly string[] Headers =
        {
            "Id", "Publication Name", "Media Type", "Media Tier",
            "Frequency", "Distribution", "Language", "CM Price", "Circulation"
        };

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

            for (int col = 1; col <= Headers.Length; col++)
            {
                var cell = ws.Cell(1, col);
                cell.Value = Headers[col - 1];
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
                ws.Cell(row, Col.Id).Value = p.Id;
                ws.Cell(row, Col.Name).Value = p.PublicationName;
                ws.Cell(row, Col.MediaType).Value = p.MediaType ?? "";
                ws.Cell(row, Col.MediaTier).Value = p.MediaTier ?? "";
                ws.Cell(row, Col.Frequency).Value = p.Frequency ?? "";
                ws.Cell(row, Col.Distribution).Value = p.Distribution ?? "";
                ws.Cell(row, Col.Language).Value = p.Language ?? "";
                ws.Cell(row, Col.CmPrice).Value = p.CmPrice;
                ws.Cell(row, Col.CmPrice).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, Col.Circulation).Value = p.Circulation.HasValue ? p.Circulation.Value.ToString("N0") : "";

                if (row % 2 == 0) ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#EBF3FB");
                ws.Range(row, 1, row, Col.Count).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
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

                // Guard against re-arranged/wrong templates before trusting column positions.
                var headerCell = ws.Cell(1, Col.Name).GetString().Trim();
                if (!string.IsNullOrEmpty(headerCell) &&
                    !headerCell.Equals("Publication Name", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Unexpected file format — please use the exported template.", 0);
                }

                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
                if (rows == null || rows.Count == 0) return (false, "The file has no data rows.", 0);

                int rowNum = 2;
                foreach (var row in rows)
                {
                    var name = row.Cell(Col.Name).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        errors.Add($"Row {rowNum}: Publication Name is required.");
                        rowNum++;
                        continue;
                    }

                    decimal cmPrice = 0;
                    var cmPriceRaw = row.Cell(Col.CmPrice).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(cmPriceRaw))
                    {
                        var cleaned = cmPriceRaw.Replace(",", "").Replace("$", "");
                        if (!decimal.TryParse(cleaned, out cmPrice))
                        {
                            errors.Add($"Row {rowNum}: CM Price '{cmPriceRaw}' is not a valid number.");
                            rowNum++;
                            continue;
                        }
                    }

                    int? circulation = null;
                    var circulationRaw = row.Cell(Col.Circulation).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(circulationRaw))
                    {
                        var cleaned = circulationRaw.Replace(",", "");
                        if (!int.TryParse(cleaned, out var parsedCirculation))
                        {
                            errors.Add($"Row {rowNum}: Circulation '{circulationRaw}' is not a valid number.");
                            rowNum++;
                            continue;
                        }
                        circulation = parsedCirculation > 0 ? parsedCirculation : null;
                    }

                    toImport.Add(new EditorEntitiesLayer.Entities.Publication
                    {
                        PublicationName = name,
                        MediaType = row.Cell(Col.MediaType).GetString().Trim().NullIfEmpty(),
                        MediaTier = row.Cell(Col.MediaTier).GetString().Trim().NullIfEmpty(),
                        Frequency = row.Cell(Col.Frequency).GetString().Trim().NullIfEmpty(),
                        Distribution = row.Cell(Col.Distribution).GetString().Trim().NullIfEmpty(),
                        Language = row.Cell(Col.Language).GetString().Trim().NullIfEmpty(),
                        CmPrice = cmPrice,
                        Circulation = circulation,
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
