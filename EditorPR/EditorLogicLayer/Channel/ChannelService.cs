using ClosedXML.Excel;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.ChannelViewModel;
using EditorEntitiesLayer.Entities;
 
namespace EditorLogicLayer.Channel
{
    public class ChannelService : IChannelService
    {
        private readonly IChannelRepository _repo;

        public ChannelService(IChannelRepository repo) => _repo = repo;

        // ── CRUD ───────────────────────────────────────────────────────────────

        public async Task<IEnumerable<ChannelDTO>> GetAllAsync()
        {
            var channels = await _repo.GetActiveChannelsAsync();
            return channels.Select(MapToDTO);
        }

        public async Task<ChannelDTO?> GetByIdAsync(int id)
        {
            var channel = await _repo.GetByIdAsync(id);
            return channel == null ? null : MapToDTO(channel);
        }

        public async Task<(bool Success, string Message)> CreateAsync(ChannelDTO model)
        {
            var entity = MapToEntity(model);

            // ✅ BaseEntity — correct field names: IsActive, CreatedAt, Deleted
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.Deleted = 0;

            await _repo.AddAsync(entity);
            return (true, "Channel created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(ChannelDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Channel not found.");

            existing.ChannelName = model.ChannelName;
            existing.MediaTier = model.MediaTier;
            existing.ChannelReach = model.ChannelReach;
            existing.Distribution = model.Distribution;
            existing.ChannelLanguage = model.ChannelLanguage;
            existing.UnitPrice = model.UnitPrice;
            existing.UnitCurrency = model.UnitCurrency;

            // ✅ BaseEntity — correct field: UpdatedAt
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Channel updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Channel not found.");

            // ✅ BaseEntity — correct fields: Deleted, IsActive, DeletedAt
            existing.Deleted = 1;
            existing.IsActive = false;
            existing.DeletedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Channel deleted successfully.");
        }

        // ── Export to Excel ────────────────────────────────────────────────────

        public byte[] ExportToExcel(IEnumerable<ChannelDTO> channels)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Channels");

            // ── Header row ────────────────────────────────────────────────────
            var headers = new[]
            {
                "Id", "Channel Name", "Media Tier", "Reach",
                "Distribution", "Language", "Unit Price", "Unit Currency"
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cell(1, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.White;
            }

            // ── Data rows ─────────────────────────────────────────────────────
            int row = 2;
            foreach (var c in channels)
            {
                ws.Cell(row, 1).Value = c.Id;
                ws.Cell(row, 2).Value = c.ChannelName;
                ws.Cell(row, 3).Value = c.MediaTier ?? "";
                ws.Cell(row, 4).Value = c.ChannelReach ?? "";
                ws.Cell(row, 5).Value = c.Distribution ?? "";
                ws.Cell(row, 6).Value = c.ChannelLanguage ?? "";
                ws.Cell(row, 7).Value = c.UnitPrice;
                ws.Cell(row, 8).Value = c.UnitCurrency;

                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";

                // Alternate row shading
                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#D6E4F0");

                ws.Range(row, 1, row, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

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
        // IFormFile is converted to Stream in the Controller only

        public async Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(
            Stream fileStream, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return (false, "Only .xlsx and .xls files are supported.", 0);

            var toImport = new List<EditorEntitiesLayer.Entities.Channel>();
            var errors = new List<string>();

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList(); // skip header row

                if (rows == null || rows.Count == 0)
                    return (false, "The file has no data rows.", 0);

                int rowNumber = 2;
                foreach (var row in rows)
                {
                    var channelName = row.Cell(2).GetString().Trim();

                    // Required field validation
                    if (string.IsNullOrWhiteSpace(channelName))
                    {
                        errors.Add($"Row {rowNumber}: Channel Name is required.");
                        rowNumber++;
                        continue;
                    }

                    // Parse decimals safely
                    decimal.TryParse(row.Cell(7).GetString().Trim(), out decimal unitPrice);
                    decimal.TryParse(row.Cell(8).GetString().Trim(), out decimal unitCurrency);

                    toImport.Add(new EditorEntitiesLayer.Entities.Channel
                    {
                        ChannelName = channelName,
                        MediaTier = row.Cell(3).GetString().Trim(),
                        ChannelReach = row.Cell(4).GetString().Trim(),
                        Distribution = row.Cell(5).GetString().Trim(),
                        ChannelLanguage = row.Cell(6).GetString().Trim(),
                        UnitPrice = unitPrice,
                        UnitCurrency = unitCurrency,

                        // ✅ BaseEntity fields set correctly
                        IsActive = true,
                        Deleted = 0,
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

            return (true, $"{toImport.Count} channel(s) imported successfully.", toImport.Count);
        }

        // ── Mappers ────────────────────────────────────────────────────────────

        private static ChannelDTO MapToDTO(EditorEntitiesLayer.Entities.Channel c) => new()
        {
            Id = c.Id,
            ChannelName = c.ChannelName,
            MediaTier = c.MediaTier,
            ChannelReach = c.ChannelReach,
            Distribution = c.Distribution,
            ChannelLanguage = c.ChannelLanguage,
            UnitPrice = c.UnitPrice,
            UnitCurrency = c.UnitCurrency
        };

        private static EditorEntitiesLayer.Entities.Channel MapToEntity(ChannelDTO dto) => new()
        {
            Id = dto.Id,
            ChannelName = dto.ChannelName,
            MediaTier = dto.MediaTier,
            ChannelReach = dto.ChannelReach,
            Distribution = dto.Distribution,
            ChannelLanguage = dto.ChannelLanguage,
            UnitPrice = dto.UnitPrice,
            UnitCurrency = dto.UnitCurrency
        };
    }
}
