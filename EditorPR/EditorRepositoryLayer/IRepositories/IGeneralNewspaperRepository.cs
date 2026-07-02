using EditorEntitiesLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EditorRepositoryLayer.IRepositories
{
    public interface IGeneralNewspaperRepository : IRepository<GeneralNewspaper>
    {
        Task<IEnumerable<GeneralNewspaper>> GetActiveAsync();
        Task<GeneralNewspaper?> GetByIdWithNavAsync(int id);   // includes Publication + Writer
        Task<IEnumerable<GeneralNewspaper>> GetByIdsAsync(IEnumerable<int> ids);

        // ── Filtered query: From/To Date, Title contains, PublicationId ───────
        Task<IEnumerable<GeneralNewspaper>> GetFilteredAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? title,
            int? publicationId);
    }
}
