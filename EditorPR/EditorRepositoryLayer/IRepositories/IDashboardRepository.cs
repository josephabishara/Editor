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


        Task<int> GetArticleCountAsync(int clientId, DateTime? from, DateTime? to);
        Task<decimal> GetArticleTotalPRAsync(int clientId, DateTime? from, DateTime? to);

        Task<int> GetNewsPaperCountAsync(int clientId, DateTime? from, DateTime? to);
        Task<decimal> GetNewsPaperTotalPRAsync(int clientId, DateTime? from, DateTime? to);

        Task<int> GetVideoCountAsync(int clientId, DateTime? from, DateTime? to);
        Task<decimal> GetVideoTotalADAsync(int clientId, DateTime? from, DateTime? to);
        Task<decimal> GetVideoTotalPRAsync(int clientId, DateTime? from, DateTime? to);

    }
}
