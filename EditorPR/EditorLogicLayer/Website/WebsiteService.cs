using ClosedXML.Excel;
using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.WebsiteViewModel;
using Microsoft.AspNetCore.Http;

namespace EditorLogicLayer.Website
{
    public class WebsiteService : IWebsiteService
    {
        private readonly IWebsiteRepository _repo;
        private readonly IClientRepository _clientRepo;
        private readonly IWebsiteCustomerCategoryRepository _categoryRepo;

        public WebsiteService(
            IWebsiteRepository repo,
            IClientRepository clientRepo,
            IWebsiteCustomerCategoryRepository categoryRepo)
        {
            _repo = repo;
            _clientRepo = clientRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IEnumerable<WebsiteDTO>> GetAllAsync()
            => (await _repo.GetActiveWebsitesAsync()).Select(MapToViewModel);

        public async Task<WebsiteDTO?> GetByIdAsync(int id)
        {
            var w = await _repo.GetByIdAsync(id);
            return w == null ? null : MapToViewModel(w);
        }

        public async Task<(bool Success, string Message)> CreateAsync(WebsiteDTO model)
        {
            var entity = MapToEntity(model);
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.Deleted = 0;

            await _repo.AddAsync(entity);
            await FanOutToClientsAsync(entity);
            return (true, "Website created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(WebsiteDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null) return (false, "Website not found.");

            existing.WebsiteName = model.WebsiteName;
            existing.URL = model.URL;
            existing.MediaTier = model.MediaTier;
            existing.Frequency = model.Frequency;
            existing.Distribution = model.Distribution;
            existing.Language = model.Language;
            existing.UnitPrice = model.UnitPrice;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Website updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            if (!await _repo.ExistsAsync(id)) return (false, "Website not found.");
            await _repo.DeleteAsync(id);
            return (true, "Website deleted successfully.");
        }

        // ── Export ─────────────────────────────────────────────────────────────

        public byte[] ExportToExcel(IEnumerable<WebsiteDTO> websites)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Websites");
            var headers = new[] { "Id", "Website Name", "URL", "Media Tier", "Frequency",  "Distribution", "Language", "Unit Price" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            int row = 2;
            foreach (var w in websites)
            {
                ws.Cell(row, 1).Value = w.Id;
                ws.Cell(row, 2).Value = w.WebsiteName;
                ws.Cell(row, 3).Value = w.URL;
                ws.Cell(row, 4).Value = w.MediaTier ?? "";
                ws.Cell(row, 5).Value = w.Frequency ?? "";
                ws.Cell(row, 7).Value = w.Distribution ?? "";
                ws.Cell(row, 8).Value = w.Language ?? "";
                ws.Cell(row, 9).Value = w.UnitPrice;
                ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";
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

        public async Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return (false, "Please select an Excel file.", 0);
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls") return (false, "Only .xlsx and .xls files are supported.", 0);

            var toImport = new List<Websites>();
            var errors = new List<string>();

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            using var workbook = new XLWorkbook(ms);
            var ws = workbook.Worksheet(1);
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
            if (rows == null || rows.Count == 0) return (false, "The Excel file has no data rows.", 0);

            int rowNum = 2;
            foreach (var row in rows)
            {
                var name = row.Cell(2).GetString().Trim();
                var url = row.Cell(3).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name)) { errors.Add($"Row {rowNum}: Website Name is required."); rowNum++; continue; }
                if (string.IsNullOrWhiteSpace(url)) { errors.Add($"Row {rowNum}: URL is required."); rowNum++; continue; }

                decimal.TryParse(row.Cell(9).GetString().Trim(), out decimal price);

                toImport.Add(new Websites
                {
                    WebsiteName = name,
                    URL = url,
                    MediaTier = row.Cell(4).GetString().Trim().NullIfEmpty(),
                    Frequency = row.Cell(5).GetString().Trim().NullIfEmpty(),
                    Distribution = row.Cell(6).GetString().Trim().NullIfEmpty(),
                    Language = row.Cell(7).GetString().Trim().NullIfEmpty(),
                    UnitPrice = price,
                    IsActive = true,
                    Deleted = 0,
                    CreatedAt = DateTime.UtcNow
                });
                rowNum++;
            }

            if (errors.Any()) return (false, string.Join(" | ", errors), 0);

            foreach (var entity in toImport)
            {
                await _repo.AddAsync(entity);
                await FanOutToClientsAsync(entity);
            }

            return (true, $"{toImport.Count} website(s) imported successfully.", toImport.Count);
        }

        // ── Fan-out helper ─────────────────────────────────────────────────────

        private async Task FanOutToClientsAsync(Websites w)
        {
            var clients = await _clientRepo.GetActiveClientsAsync();
            if (!clients.Any()) return;

            var rows = clients.Select(c => new WebsiteCustomerCategory
            {
                CustomerId = c.Id,
                WebsiteId = w.Id,
                MediaTier = w.MediaTier,
                Frequency = w.Frequency,
                Distribution = w.Distribution,
                Language = w.Language,
                UnitPrice = w.UnitPrice
            }).ToList();

            await _categoryRepo.AddRangeAsync(rows);
        }

        // ── Mappers ────────────────────────────────────────────────────────────

        private static WebsiteDTO MapToViewModel(Websites w) => new()
        {
            Id = w.Id,
            WebsiteName = w.WebsiteName,
            URL = w.URL,
            MediaTier = w.MediaTier,
            Frequency = w.Frequency,
            Distribution = w.Distribution,
            Language = w.Language,
            UnitPrice = w.UnitPrice
        };

        private static Websites MapToEntity(WebsiteDTO vm) => new()
        {
            Id = vm.Id,
            WebsiteName = vm.WebsiteName,
            URL = vm.URL,
            MediaTier = vm.MediaTier,
            Frequency = vm.Frequency,
            Distribution = vm.Distribution,
            Language = vm.Language,
            UnitPrice = vm.UnitPrice
        };
    }

    // ── String helper (local to LogicLayer) ────────────────────────────────────
    internal static class StringEx
    {
        public static string? NullIfEmpty(this string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
