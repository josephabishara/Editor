using EditorViewModelLayer.WebsiteViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Website
{
    public interface IWebsiteService
    {
        Task<IEnumerable<WebsiteDTO>> GetAllAsync();
        Task<WebsiteDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(WebsiteDTO model);
        Task<(bool Success, string Message)> UpdateAsync(WebsiteDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        byte[] ExportToExcel(IEnumerable<WebsiteDTO> websites);
        Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(IFormFile file);

    }
}
