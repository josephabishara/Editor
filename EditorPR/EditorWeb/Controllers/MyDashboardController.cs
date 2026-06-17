using ClosedXML.Excel;
using EditorLogicLayer.Client;
using EditorLogicLayer.ClientArticleLogic;
using EditorLogicLayer.ClientNewsPaperLogic;
using EditorLogicLayer.ClientVideoLogic;
using EditorLogicLayer.Dashboard;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.MediaViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EditorWeb.Controllers
{
    /// <summary>
    /// Client-facing portal. Accessible only by roles: Client, Assistant.
    /// The logged-in user's ClientId is resolved once per action via
    /// <see cref="IClientResolverService"/> and never accepted from query-string
    /// to prevent IDOR attacks.
    /// </summary>
    [Authorize(Roles = "Client,Assistant")]
    public class MyDashboardController : Controller
    {
        private readonly ITheClientResolverService _resolver;
        private readonly IDashboardService _dashService;
        private readonly IClientArticleService _articleService;
        private readonly IClientNewsPaperService _paperService;
        private readonly IClientVideoService _videoService;

        public MyDashboardController(
            ITheClientResolverService resolver,
            IDashboardService dashService,
            IClientArticleService articleService,
            IClientNewsPaperService paperService,
            IClientVideoService videoService)
        {
            _resolver = resolver;
            _dashService = dashService;
            _articleService = articleService;
            _paperService = paperService;
            _videoService = videoService;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the ClientId for the current user.
        /// Returns null when no Client/Assistant record is linked.
        /// </summary>
        private async Task<int?> GetClientIdAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return null;

            return await _resolver.ResolveClientIdAsync(userId);
        }

        private IActionResult ClientNotFound()
            => View("Error", "Your account is not linked to a client record. Please contact support.");

        // ═════════════════════════════════════════════════════════════════════
        // DASHBOARD
        // ═════════════════════════════════════════════════════════════════════

        // GET /MyDashboard/Index
        public async Task<IActionResult> Index(string? dateFrom = null, string? dateTo = null)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            DateTime? from = DateTime.TryParse(dateFrom, out var f) ? f : null;
            DateTime? to = DateTime.TryParse(dateTo, out var t) ? t : null;

            var model = (from.HasValue || to.HasValue)
                ? await _dashService.GetClientDashboardAsync(clientId.Value, from, to)
                : await _dashService.GetClientDashboardAsync(clientId.Value);

            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            return View("ClientDashboard", model);
        }

        // ═════════════════════════════════════════════════════════════════════
        // CLIENT ARTICLE
        // ═════════════════════════════════════════════════════════════════════

        // GET /MyDashboard/ClientArticle
        public async Task<IActionResult> ClientArticle(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int writerId = 0,
            int websiteId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _articleService.GetListAsync(clientId.Value);
            var items = list.Items.AsEnumerable();

            // ── Filters ──────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(title))
                items = items.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(dateFrom, out var from))
                items = items.Where(i => i.Date.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to))
                items = items.Where(i => i.Date.Date <= to.Date);
            if (categoryId > 0) items = items.Where(i => i.CategoryId == categoryId);
            if (subCategoryId > 0) items = items.Where(i => i.SubCategoryId == subCategoryId);
            if (writerId > 0) items = items.Where(i => i.WriterId == writerId);
            if (websiteId > 0) items = items.Where(i => i.WebsiteId == websiteId);

            list.Items = items.ToList();
            list.WebsiteOptions = await _articleService.GetWebsiteOptionsAsync(websiteId);
            list.CategoryOptions = await _articleService.GetCategoryOptionsAsync(clientId.Value, categoryId);
            list.SubCategoryOptions = categoryId > 0
                ? await _articleService.GetSubCategoryOptionsAsync(categoryId, subCategoryId)
                : new List<MediaSelectOption>();
            list.WriterOptions = await _articleService.GetWriterOptionsAsync(writerId);

            ViewBag.FilterTitle = title;
            ViewBag.FilterDateFrom = dateFrom;
            ViewBag.FilterDateTo = dateTo;
            ViewBag.FilterCategoryId = categoryId;
            ViewBag.FilterSubId = subCategoryId;
            ViewBag.FilterWriterId = writerId;
            ViewBag.FilterWebsiteId = websiteId;

            return View(list);
        }

        // GET /MyDashboard/ArticleDetails/{id}
        public async Task<IActionResult> ArticleDetails(int id)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var dto = await _articleService.GetByIdAsync(id);
            if (dto == null || dto.ClientId != clientId.Value)
                return NotFound();

            return View(dto);
        }

        // GET /MyDashboard/ExportArticlesExcel
        public async Task<IActionResult> ExportArticlesExcel(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int writerId = 0,
            int websiteId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _articleService.GetListAsync(clientId.Value);
            var items = ApplyArticleFilters(list.Items, title, dateFrom, dateTo,
                            categoryId, subCategoryId, writerId, websiteId);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Articles");

            // Header
            string[] headers = {
                "#", "Date", "Title", "Website", "Category", "Sub Category",
                "Writer", "Media Type", "Frequency", "Impression", "Reach",
                "AD Value", "PR Value", "Toning", "Language"
            };
            for (int col = 1; col <= headers.Length; col++)
            {
                ws.Cell(1, col).Value = headers[col - 1];
                ws.Cell(1, col).Style.Font.Bold = true;
                ws.Cell(1, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a2e5a");
                ws.Cell(1, col).Style.Font.FontColor = XLColor.White;
            }

            // Rows
            int row = 2;
            foreach (var a in items)
            {
                ws.Cell(row, 1).Value = row - 1;
                ws.Cell(row, 2).Value = a.Date.ToString("yyyy-MM-dd");
                ws.Cell(row, 3).Value = a.Title;
                ws.Cell(row, 4).Value = a.WebsiteName ?? "";
                ws.Cell(row, 5).Value = a.CategoryName ?? "";
                ws.Cell(row, 6).Value = a.SubCategoryName ?? "";
                ws.Cell(row, 7).Value = a.WriterName ?? "";
                ws.Cell(row, 8).Value = a.MediaType ?? "";
                ws.Cell(row, 9).Value = a.Frequency ?? "";
                ws.Cell(row, 10).Value = a.Impression;
                ws.Cell(row, 11).Value = a.Reach;
                ws.Cell(row, 12).Value = a.ADValue;
                ws.Cell(row, 13).Value = a.PRValue;
                ws.Cell(row, 14).Value = a.Toning ?? "";
                ws.Cell(row, 15).Value = a.Language ?? "";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f7fa");
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Articles_{list.ClientName}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // GET /MyDashboard/ExportArticlesPdf
        public async Task<IActionResult> ExportArticlesPdf(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int writerId = 0,
            int websiteId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _articleService.GetListAsync(clientId.Value);
            var items = ApplyArticleFilters(list.Items, title, dateFrom, dateTo,
                            categoryId, subCategoryId, writerId, websiteId);

            ViewBag.ClientName = list.ClientName;
            ViewBag.ExportDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View("ExportArticlesPdf", items);
        }

        // ═════════════════════════════════════════════════════════════════════
        // CLIENT NEWSPAPER
        // ═════════════════════════════════════════════════════════════════════

        // GET /MyDashboard/ClientNewsPaper
        public async Task<IActionResult> ClientNewsPaper(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int writerId = 0,
            int publicationId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _paperService.GetListAsync(clientId.Value);
            var items = list.Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
                items = items.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(dateFrom, out var from))
                items = items.Where(i => i.Date.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to))
                items = items.Where(i => i.Date.Date <= to.Date);
            if (categoryId > 0) items = items.Where(i => i.CategoryId == categoryId);
            if (subCategoryId > 0) items = items.Where(i => i.SubCategoryId == subCategoryId);
            if (writerId > 0) items = items.Where(i => i.WriterId == writerId);
            if (publicationId > 0) items = items.Where(i => i.PublicationId == publicationId);

            list.Items = items.ToList();
            list.PublicationOptions = await _paperService.GetPublicationOptionsAsync(publicationId);
            list.CategoryOptions = await _paperService.GetCategoryOptionsAsync(clientId.Value, categoryId);
            list.SubCategoryOptions = categoryId > 0
                ? await _paperService.GetSubCategoryOptionsAsync(categoryId, subCategoryId)
                : new List<MediaSelectOption>();
            list.WriterOptions = await _paperService.GetWriterOptionsAsync(writerId);

            ViewBag.FilterTitle = title;
            ViewBag.FilterDateFrom = dateFrom;
            ViewBag.FilterDateTo = dateTo;
            ViewBag.FilterCategoryId = categoryId;
            ViewBag.FilterSubId = subCategoryId;
            ViewBag.FilterWriterId = writerId;
            ViewBag.FilterPublicationId = publicationId;

            return View(list);
        }

        // GET /MyDashboard/NewsPaperDetails/{id}
        public async Task<IActionResult> NewsPaperDetails(int id)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var dto = await _paperService.GetByIdAsync(id);
            if (dto == null || dto.ClientId != clientId.Value)
                return NotFound();

            return View(dto);
        }

        // GET /MyDashboard/ExportNewsPaperExcel
        public async Task<IActionResult> ExportNewsPaperExcel(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int writerId = 0,
            int publicationId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _paperService.GetListAsync(clientId.Value);
            var items = ApplyPaperFilters(list.Items, title, dateFrom, dateTo,
                            categoryId, subCategoryId, writerId, publicationId);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Newspapers");

            string[] headers = {
                "#", "Date", "Title", "Publication", "Category", "Sub Category",
                "Writer", "Media Type", "Frequency", "Circulation", "Reach",
                "AD Value", "PR Value", "Toning", "Language", "Pages"
            };
            for (int col = 1; col <= headers.Length; col++)
            {
                ws.Cell(1, col).Value = headers[col - 1];
                ws.Cell(1, col).Style.Font.Bold = true;
                ws.Cell(1, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a2e5a");
                ws.Cell(1, col).Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var p in items)
            {
                ws.Cell(row, 1).Value = row - 1;
                ws.Cell(row, 2).Value = p.Date.ToString("yyyy-MM-dd");
                ws.Cell(row, 3).Value = p.Title;
                ws.Cell(row, 4).Value = p.PublicationName ?? "";
                ws.Cell(row, 5).Value = p.CategoryName ?? "";
                ws.Cell(row, 6).Value = p.SubCategoryName ?? "";
                ws.Cell(row, 7).Value = p.WriterName ?? "";
                ws.Cell(row, 8).Value = p.MediaType ?? "";
                ws.Cell(row, 9).Value = p.Frequency ?? "";
                ws.Cell(row, 10).Value = p.Circulation ?? 0;
                ws.Cell(row, 11).Value = p.Reach;
                ws.Cell(row, 12).Value = p.ADValue;
                ws.Cell(row, 13).Value = p.PRValue;
                ws.Cell(row, 14).Value = p.Toning ?? "";
                ws.Cell(row, 15).Value = p.Language ?? "";
                ws.Cell(row, 16).Value = p.Pages;

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f7fa");
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Newspapers_{list.ClientName}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // GET /MyDashboard/ExportNewsPaperPdf
        public async Task<IActionResult> ExportNewsPaperPdf(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int writerId = 0,
            int publicationId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _paperService.GetListAsync(clientId.Value);
            var items = ApplyPaperFilters(list.Items, title, dateFrom, dateTo,
                            categoryId, subCategoryId, writerId, publicationId);

            ViewBag.ClientName = list.ClientName;
            ViewBag.ExportDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View("ExportNewsPaperPdf", items);
        }

        // ═════════════════════════════════════════════════════════════════════
        // CLIENT VIDEO
        // ═════════════════════════════════════════════════════════════════════

        // GET /MyDashboard/ClientVideo
        public async Task<IActionResult> ClientVideo(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int channelId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _videoService.GetListAsync(clientId.Value);
            var items = list.Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(title))
                items = items.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(dateFrom, out var from))
                items = items.Where(i => i.Date.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to))
                items = items.Where(i => i.Date.Date <= to.Date);
            if (categoryId > 0) items = items.Where(i => i.CategoryId == categoryId);
            if (subCategoryId > 0) items = items.Where(i => i.SubCategoryId == subCategoryId);
            if (channelId > 0) items = items.Where(i => i.ChannelId == channelId);

            list.Items = items.ToList();
            list.ChannelOptions = await _videoService.GetChannelOptionsAsync(channelId);
            list.CategoryOptions = await _videoService.GetCategoryOptionsAsync(clientId.Value, categoryId);
            list.SubCategoryOptions = categoryId > 0
                ? await _videoService.GetSubCategoryOptionsAsync(categoryId, subCategoryId)
                : new List<MediaSelectOption>();

            ViewBag.FilterTitle = title;
            ViewBag.FilterDateFrom = dateFrom;
            ViewBag.FilterDateTo = dateTo;
            ViewBag.FilterCategoryId = categoryId;
            ViewBag.FilterSubId = subCategoryId;
            ViewBag.FilterChannelId = channelId;

            return View(list);
        }

        // GET /MyDashboard/VideoDetails/{id}
        public async Task<IActionResult> VideoDetails(int id)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var dto = await _videoService.GetByIdAsync(id);
            if (dto == null || dto.ClientId != clientId.Value)
                return NotFound();

            return View(dto);
        }

        // GET /MyDashboard/ExportVideosExcel
        public async Task<IActionResult> ExportVideosExcel(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int channelId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _videoService.GetListAsync(clientId.Value);
            var items = ApplyVideoFilters(list.Items, title, dateFrom, dateTo,
                            categoryId, subCategoryId, channelId);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Videos");

            string[] headers = {
                "#", "Date", "Title", "Channel", "Category", "Sub Category",
                "Program", "Duration (s)", "Impression", "Reach",
                "AD Value", "PR Value", "Toning", "Language"
            };
            for (int col = 1; col <= headers.Length; col++)
            {
                ws.Cell(1, col).Value = headers[col - 1];
                ws.Cell(1, col).Style.Font.Bold = true;
                ws.Cell(1, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a2e5a");
                ws.Cell(1, col).Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var v in items)
            {
                ws.Cell(row, 1).Value = row - 1;
                ws.Cell(row, 2).Value = v.Date.ToString("yyyy-MM-dd");
                ws.Cell(row, 3).Value = v.Title;
                ws.Cell(row, 4).Value = v.ChannelName ?? "";
                ws.Cell(row, 5).Value = v.CategoryName ?? "";
                ws.Cell(row, 6).Value = v.SubCategoryName ?? "";
                ws.Cell(row, 7).Value = v.Program ?? "";
                ws.Cell(row, 8).Value = v.Duration;
                ws.Cell(row, 9).Value = v.Impression;
                ws.Cell(row, 10).Value = v.Reach;
                ws.Cell(row, 11).Value = v.ADValue;
                ws.Cell(row, 12).Value = v.PRValue;
                ws.Cell(row, 13).Value = v.Toning ?? "";
                ws.Cell(row, 14).Value = v.Language ?? "";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f5f7fa");
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Videos_{list.ClientName}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // GET /MyDashboard/ExportVideosPdf
        public async Task<IActionResult> ExportVideosPdf(
            string? title = null,
            string? dateFrom = null,
            string? dateTo = null,
            int categoryId = 0,
            int subCategoryId = 0,
            int channelId = 0)
        {
            var clientId = await GetClientIdAsync();
            if (clientId == null) return ClientNotFound();

            var list = await _videoService.GetListAsync(clientId.Value);
            var items = ApplyVideoFilters(list.Items, title, dateFrom, dateTo,
                            categoryId, subCategoryId, channelId);

            ViewBag.ClientName = list.ClientName;
            ViewBag.ExportDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View("ExportVideosPdf", items);
        }

        // ═════════════════════════════════════════════════════════════════════
        // PRIVATE FILTER HELPERS
        // ═════════════════════════════════════════════════════════════════════

        private static IEnumerable<ClientArticleDTO> ApplyArticleFilters(
            IEnumerable<ClientArticleDTO> src,
            string? title, string? dateFrom, string? dateTo,
            int categoryId, int subCategoryId, int writerId, int websiteId)
        {
            var q = src;
            if (!string.IsNullOrWhiteSpace(title))
                q = q.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(dateFrom, out var from)) q = q.Where(i => i.Date.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to)) q = q.Where(i => i.Date.Date <= to.Date);
            if (categoryId > 0) q = q.Where(i => i.CategoryId == categoryId);
            if (subCategoryId > 0) q = q.Where(i => i.SubCategoryId == subCategoryId);
            if (writerId > 0) q = q.Where(i => i.WriterId == writerId);
            if (websiteId > 0) q = q.Where(i => i.WebsiteId == websiteId);
            return q;
        }

        private static IEnumerable<ClientNewsPaperDTO> ApplyPaperFilters(
            IEnumerable<ClientNewsPaperDTO> src,
            string? title, string? dateFrom, string? dateTo,
            int categoryId, int subCategoryId, int writerId, int publicationId)
        {
            var q = src;
            if (!string.IsNullOrWhiteSpace(title))
                q = q.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(dateFrom, out var from)) q = q.Where(i => i.Date.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to)) q = q.Where(i => i.Date.Date <= to.Date);
            if (categoryId > 0) q = q.Where(i => i.CategoryId == categoryId);
            if (subCategoryId > 0) q = q.Where(i => i.SubCategoryId == subCategoryId);
            if (writerId > 0) q = q.Where(i => i.WriterId == writerId);
            if (publicationId > 0) q = q.Where(i => i.PublicationId == publicationId);
            return q;
        }

        private static IEnumerable<ClientVideoDTO> ApplyVideoFilters(
            IEnumerable<ClientVideoDTO> src,
            string? title, string? dateFrom, string? dateTo,
            int categoryId, int subCategoryId, int channelId)
        {
            var q = src;
            if (!string.IsNullOrWhiteSpace(title))
                q = q.Where(i => i.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(dateFrom, out var from)) q = q.Where(i => i.Date.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to)) q = q.Where(i => i.Date.Date <= to.Date);
            if (categoryId > 0) q = q.Where(i => i.CategoryId == categoryId);
            if (subCategoryId > 0) q = q.Where(i => i.SubCategoryId == subCategoryId);
            if (channelId > 0) q = q.Where(i => i.ChannelId == channelId);
            return q;
        }
    }
}
