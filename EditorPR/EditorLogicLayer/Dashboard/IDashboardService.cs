using EditorViewModelLayer.ClientViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Dashboard
{
    public interface IDashboardService
    {
        Task<ClientDashboardDTO> GetClientDashboardAsync(int clientId);
    }
}
