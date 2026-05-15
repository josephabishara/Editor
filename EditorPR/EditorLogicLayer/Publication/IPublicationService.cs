using EditorViewModelLayer.PublicationViewModel;
using EditorViewModelLayer.WebsiteViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorLogicLayer.Publication
{
    public interface IPublicationService
    {
        Task<IEnumerable<PublicationDTO>> GetAllAsync();
        Task<PublicationDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(PublicationDTO model);
        Task<(bool Success, string Message)> UpdateAsync(PublicationDTO model);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        byte[] ExportToExcel(IEnumerable<PublicationDTO> publications);
        Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(Stream fileStream, string fileName);


    }
}
