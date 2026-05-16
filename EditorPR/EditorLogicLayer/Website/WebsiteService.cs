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
        {
            var websites = await _repo.GetActiveWebsitesAsync();
            return websites.Select(MapToViewModel);
        }

        public async Task<WebsiteDTO?> GetByIdAsync(int id)
        {
            var website = await _repo.GetByIdAsync(id);
            return website == null ? null : MapToViewModel(website);
        }

        //public async Task<(bool Success, string Message)> CreateAsync(WebsiteDTO model)
        //{
        //    var entity = MapToEntity(model);
        //    await _repo.AddAsync(entity);
        //    return (true, "Website created successfully.");
        //}
        public async Task<(bool Success, string Message)> CreateAsync(WebsiteDTO model)
        {
            var entity = MapToEntity(model);
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            entity.Deleted = 0;

            await _repo.AddAsync(entity);

            // Fan-out: create one WebsiteCustomerCategory row per active client,
            // inheriting this website's MediaTier as the default (editable later).
            await FanOutToClientsAsync(entity.Id, entity.MediaTier);

            return (true, "Website created successfully.");
        }
        public async Task<(bool Success, string Message)> UpdateAsync(WebsiteDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Website not found.");

            existing.WebsiteName = model.WebsiteName;
            existing.URL = model.URL;
            existing.MediaTier = model.MediaTier;
            existing.Frequency = model.Frequency;
            existing.Reach = model.Reach;
            existing.Distribution = model.Distribution;
            existing.Language = model.Language;
            existing.UnitPrice = model.UnitPrice;

            await _repo.UpdateAsync(existing);
            return (true, "Website updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            if (!await _repo.ExistsAsync(id))
                return (false, "Website not found.");

            await _repo.DeleteAsync(id);
            return (true, "Website deleted successfully.");
        }

        // ── Export to Excel ────────────────────────────────────────────────────

        public byte[] ExportToExcel(IEnumerable<WebsiteDTO> websites)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Websites");

            // ── Header row ────────────────────────────────────────────────
            var headers = new[]
            {
                "Id", "Website Name", "URL", "Media Tier",
                "Frequency", "Reach", "Distribution", "Language", "Unit Price"
            };

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

            // ── Data rows ─────────────────────────────────────────────────
            int row = 2;
            foreach (var w in websites)
            {
                ws.Cell(row, 1).Value = w.Id;
                ws.Cell(row, 2).Value = w.WebsiteName;
                ws.Cell(row, 3).Value = w.URL;
                ws.Cell(row, 4).Value = w.MediaTier ?? "";
                ws.Cell(row, 5).Value = w.Frequency ?? "";
                ws.Cell(row, 6).Value = w.Reach ?? "";
                ws.Cell(row, 7).Value = w.Distribution ?? "";
                ws.Cell(row, 8).Value = w.Language ?? "";
                ws.Cell(row, 9).Value = w.UnitPrice;
                ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";

                // Alternate row color
                if (row % 2 == 0)
                {
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#EBF3FB");
                }

                // Border on all cells
                ws.Range(row, 1, row, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            // ── Auto-fit columns ──────────────────────────────────────────
            ws.Columns().AdjustToContents();

            // ── Freeze header row ─────────────────────────────────────────
            ws.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ── Import from Excel ──────────────────────────────────────────────────

        public async Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(IFormFile file)
        {
            // Validate file
            if (file == null || file.Length == 0)
                return (false, "Please select an Excel file.", 0);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return (false, "Only .xlsx and .xls files are supported.", 0);

            var importedRows = new List<Websites>();
            var errors = new List<string>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1); // First sheet

            var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList(); // Skip header row
            if (rows == null || rows.Count == 0)
                return (false, "The Excel file has no data rows.", 0);

            int rowNumber = 2; // Start after header
            foreach (var row in rows)
            {
                try
                {
                    var websiteName = row.Cell(2).GetString().Trim();
                    var url = row.Cell(3).GetString().Trim();

                    // Required field validation
                    if (string.IsNullOrWhiteSpace(websiteName))
                    {
                        errors.Add($"Row {rowNumber}: Website Name is required.");
                        rowNumber++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(url))
                    {
                        errors.Add($"Row {rowNumber}: URL is required.");
                        rowNumber++;
                        continue;
                    }

                    // Parse UnitPrice safely
                    decimal unitPrice = 0;
                    var priceCell = row.Cell(9).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(priceCell))
                        decimal.TryParse(priceCell, out unitPrice);

                    importedRows.Add(new Websites
                    {
                        WebsiteName = websiteName,
                        URL = url,
                        MediaTier = row.Cell(4).GetString().Trim(),
                        Frequency = row.Cell(5).GetString().Trim(),
                        Reach = row.Cell(6).GetString().Trim(),
                        Distribution = row.Cell(7).GetString().Trim(),
                        Language = row.Cell(8).GetString().Trim(),
                        UnitPrice = unitPrice,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {rowNumber}: {ex.Message}");
                }

                rowNumber++;
            }

            if (errors.Any())
                return (false, string.Join(" | ", errors), 0);

            // Bulk insert
            foreach (var entity in importedRows)
                await _repo.AddAsync(entity);

            return (true, $"{importedRows.Count} website(s) imported successfully.", importedRows.Count);
        }

        /// <summary>
        /// For a newly persisted website, creates one <see cref="WebsiteCustomerCategory"/>
        /// row for every active client, defaulting MediaTier from the website itself.
        /// Clients that already have a row for this website are skipped (safe to call on
        /// re-import scenarios, though normally each website is new).
        /// </summary>
        private async Task FanOutToClientsAsync(int websiteId, string? mediaTier)
        {
            var clients = await _clientRepo.GetActiveClientsAsync();
            if (!clients.Any()) return;

            var categories = clients.Select(c => new WebsiteCustomerCategory
            {
                CustomerId = c.Id,
                WebsiteId = websiteId,
                MediaTier = mediaTier   // default from website — editable per-client later
            }).ToList();

            await _categoryRepo.AddRangeAsync(categories);
        }

        private static WebsiteDTO MapToViewModel(Websites w) => new()
        {
            Id = w.Id,
            WebsiteName = w.WebsiteName,
            URL = w.URL,
            MediaTier = w.MediaTier,
            Frequency = w.Frequency,
            Reach = w.Reach,
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
            Reach = vm.Reach,
            Distribution = vm.Distribution,
            Language = vm.Language,
            UnitPrice = vm.UnitPrice
        };
    }
}
