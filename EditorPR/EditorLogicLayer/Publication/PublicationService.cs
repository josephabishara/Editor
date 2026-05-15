using ClosedXML.Excel;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.PublicationViewModel;

namespace EditorLogicLayer.Publication
{
    public class PublicationService : IPublicationService
    {
        private readonly IPublicationRepository _repo;

        public PublicationService(IPublicationRepository repo) => _repo = repo;

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

            // ✅ BaseEntity fields — CreatedAt, IsActive, Deleted
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            entity.Deleted = 0;

            await _repo.AddAsync(entity);
            return (true, "Publication created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(PublicationDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Publication not found.");

            existing.PublicationName = model.PublicationName;
            existing.URL = model.URL;
            existing.MediaType = model.MediaType;
            existing.MediaTier = model.MediaTier;
            existing.Frequency = model.Frequency;
            existing.Reach = model.Reach;
            existing.Distribution = model.Distribution;
            existing.Language = model.Language;
            existing.CmPrice = model.CmPrice;
            existing.Circulation = model.Circulation;

            // ✅ BaseEntity field — UpdatedAt (not UpdatedDate, not UpdatedAt mismatch)
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Publication updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Publication not found.");

            // ✅ Soft delete — BaseEntity fields: Deleted, IsActive, DeletedAt
            existing.Deleted = 1;
            existing.IsActive = false;
            existing.DeletedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Publication deleted successfully.");
        }
        // ── Export to Excel ────────────────────────────────────────────────────

        public byte[] ExportToExcel(IEnumerable<PublicationDTO> publications)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Publications");

            // ── Header row ────────────────────────────────────────────────────
            var headers = new[]
            {
                "Id", "Publication Name", "URL", "Media Type", "Media Tier",
                "Frequency", "Reach", "Distribution", "Language", "CM Price" ,"Circulation"
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

            // ── Data rows ─────────────────────────────────────────────────────
            int row = 2;
            foreach (var w in publications)
            {
                ws.Cell(row, 1).Value = w.Id;
                ws.Cell(row, 2).Value = w.PublicationName;
                ws.Cell(row, 3).Value = w.URL;
                ws.Cell(row, 4).Value = w.MediaType ?? "";
                ws.Cell(row, 5).Value = w.MediaTier ?? "";
                ws.Cell(row, 6).Value = w.Frequency ?? "";
                ws.Cell(row, 7).Value = w.Reach ?? "";
                ws.Cell(row, 8).Value = w.Distribution ?? "";
                ws.Cell(row, 9).Value = w.Language ?? "";
                ws.Cell(row, 10).Value = w.CmPrice;
                ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 11).Value = w.Circulation.HasValue ? w.Circulation.Value.ToString("N0") : "—";

                // Alternate row shading
                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#EBF3FB");

                ws.Range(row, 1, row, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ── Import from Excel ──────────────────────────────────────────────────
        // ✅ Receives Stream + fileName — NOT IFormFile
        // IFormFile is an ASP.NET Core type — LogicLayer must not reference it
        // Conversion from IFormFile → Stream happens in the Controller only

        public async Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(
            Stream fileStream, string fileName)
        {
            // Validate extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return (false, "Only .xlsx and .xls files are supported.", 0);

            var toImport = new List<EditorEntitiesLayer.Entities.Publication>();
            var errors = new List<string>();

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList(); // skip header

                if (rows == null || rows.Count == 0)
                    return (false, "The file has no data rows.", 0);

                int rowNumber = 2;
                foreach (var row in rows)
                {
                    var publicationName = row.Cell(2).GetString().Trim();
                    var url = row.Cell(3).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(publicationName))
                    {
                        errors.Add($"Row {rowNumber}: Publication Name is required.");
                        rowNumber++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(url))
                    {
                        errors.Add($"Row {rowNumber}: URL is required.");
                        rowNumber++;
                        continue;
                    }

                    decimal.TryParse(row.Cell(9).GetString().Trim(), out decimal cmPrice);

                    toImport.Add(new EditorEntitiesLayer.Entities.Publication
                    {
                        PublicationName = publicationName,
                        URL = url,
                        MediaType = row.Cell(4).GetString().Trim(),
                        MediaTier = row.Cell(5).GetString().Trim(),
                        Frequency = row.Cell(6).GetString().Trim(),
                        Reach = row.Cell(7).GetString().Trim(),
                        Distribution = row.Cell(8).GetString().Trim(),
                        Language = row.Cell(9).GetString().Trim(),
                        CmPrice = cmPrice,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });

                    rowNumber++;
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

            return (true, $"{toImport.Count} publication(s) imported successfully.", toImport.Count);
        }
        // ── Mappers ────────────────────────────────────────────────────────────

        private static PublicationDTO MapToDTO(EditorEntitiesLayer.Entities.Publication p) => new()
        {
            Id = p.Id,
            PublicationName = p.PublicationName,
            URL = p.URL,
            MediaType = p.MediaType,
            MediaTier = p.MediaTier,
            Frequency = p.Frequency,
            Reach = p.Reach,
            Distribution = p.Distribution,
            Language = p.Language,
            CmPrice = p.CmPrice,
            Circulation = p.Circulation
        };

        private static EditorEntitiesLayer.Entities.Publication MapToEntity(PublicationDTO dto) => new()
        {
            Id = dto.Id,
            PublicationName = dto.PublicationName,
            URL = dto.URL,
            MediaType = dto.MediaType,
            MediaTier = dto.MediaTier,
            Frequency = dto.Frequency,
            Reach = dto.Reach,
            Distribution = dto.Distribution,
            Language = dto.Language,
            CmPrice = dto.CmPrice,
            Circulation = dto.Circulation
        };
    }
}
