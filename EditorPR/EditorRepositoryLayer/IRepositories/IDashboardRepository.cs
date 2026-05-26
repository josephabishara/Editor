using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IDashboardRepository
    {
        Task<int> GetArticleCountAsync(int clientId);
        Task<decimal> GetArticleTotalPRAsync(int clientId);

        Task<int> GetNewsPaperCountAsync(int clientId);
        Task<decimal> GetNewsPaperTotalPRAsync(int clientId);

        Task<int> GetVideoCountAsync(int clientId);
        Task<decimal> GetVideoTotalADAsync(int clientId);
        Task<decimal> GetVideoTotalPRAsync(int clientId);
    }
}
