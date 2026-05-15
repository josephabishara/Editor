using EditorEntitiesLayer.Entities;
using EditorViewModelLayer.WebsiteViewModel;
using EditorViewModelLayer.WriterViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Writer
{
    public interface IWriterService
    {
        Task<IEnumerable<WriterDTO>> GetAllAsync();
        Task<WriterDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(WriterDTO model);
        Task<(bool Success, string Message)> UpdateAsync(WriterDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }
}
