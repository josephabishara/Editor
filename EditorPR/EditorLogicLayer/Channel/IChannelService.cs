using EditorViewModelLayer.ChannelViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Channel
{
    public interface IChannelService
    {
        // ── CRUD ───────────────────────────────────────────────────────────────
        Task<IEnumerable<ChannelDTO>> GetAllAsync();
        Task<ChannelDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(ChannelDTO model);
        Task<(bool Success, string Message)> UpdateAsync(ChannelDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);

        // ── Excel ──────────────────────────────────────────────────────────────
        // ✅ Returns byte[] — no ASP.NET Core dependency
        byte[] ExportToExcel(IEnumerable<ChannelDTO> channels);

        // ✅ Receives Stream + fileName — NOT IFormFile (LogicLayer stays clean)
        Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(Stream fileStream, string fileName);
    }
}
