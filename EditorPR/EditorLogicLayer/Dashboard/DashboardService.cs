using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.ClientViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IClientRepository _clientRepo;
        private readonly IDashboardRepository _dashRepo;

        public DashboardService(
            IClientRepository clientRepo,
            IDashboardRepository dashRepo)
        {
            _clientRepo = clientRepo;
            _dashRepo = dashRepo;
        }

        public async Task<ClientDashboardDTO> GetClientDashboardAsync(int clientId)
        {
            var client = await _clientRepo.GetByIdAsync(clientId)
                         ?? throw new KeyNotFoundException($"Client {clientId} not found.");

            return new ClientDashboardDTO
            {
                ClientId = client.Id,
                Name = client.Name,
                Photo = client.Photo,
                Email = client.Email,
                Notes = client.Notes,

                ArticleCount = await _dashRepo.GetArticleCountAsync(clientId),
                ArticleTotalPR = await _dashRepo.GetArticleTotalPRAsync(clientId),

                NewsPaperCount = await _dashRepo.GetNewsPaperCountAsync(clientId),
                NewsPaperTotalPR = await _dashRepo.GetNewsPaperTotalPRAsync(clientId),

                VideoCount = await _dashRepo.GetVideoCountAsync(clientId),
                VideoTotalAD = await _dashRepo.GetVideoTotalADAsync(clientId),
                VideoTotalPR = await _dashRepo.GetVideoTotalPRAsync(clientId),
            };
        }
    }
}
