using EditorLogicLayer.Client;
using EditorLogicLayer.Dashboard;
using EditorLogicLayer.News;
using EditorViewModelLayer.ClientViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,Auditor")]

    public class DashboardController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IClientNewsService _newsService;
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            IClientService clientService,
            IClientNewsService newsService,
            IDashboardService dashboardService)
        {
            _clientService = clientService;
            _newsService = newsService;
            _dashboardService = dashboardService;
        }

        // GET: /Client
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllAsync();
            return View(clients);
        }

        // ClientDashboard
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        [HttpGet]
        public async Task<IActionResult> ClientDashboard(int id)
        {
            try
            {
                var dashboard = await _dashboardService.GetClientDashboardAsync(id);
                return View(dashboard);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

        }
    }
}
