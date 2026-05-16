using EditorLogicLayer.Client;
using EditorViewModelLayer.ClientViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EditorWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,Auditor")]

    public class DashboardController : Controller
    {
        private readonly IClientService _clientService;

        public DashboardController(IClientService clientService)
            => _clientService = clientService;

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
        public async Task<IActionResult> ClientDashboard(int id)
        {
            ClientDTO client = await _clientService.GetByIdAsync(id);
            ClientDashboardDTO clientDashboard = new ClientDashboardDTO();
          
            clientDashboard.Publications = 50;
            clientDashboard.Websites = 10;
            clientDashboard.Videos = 20;
            clientDashboard.TotalAD = 30;
            clientDashboard.TotalPR = 40;
            clientDashboard.TotalCirclation = 60;
            clientDashboard.Name = client.Name;
            clientDashboard.Photo = client.Photo;
            clientDashboard.Email = client.Email;
            clientDashboard.Notes = client.Notes;
            clientDashboard.ClientId = client.Id;

            return View(clientDashboard);
        }
    }
}
