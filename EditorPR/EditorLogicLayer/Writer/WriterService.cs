using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.WriterViewModel;
 

namespace EditorLogicLayer.Writer
{
    public class WriterService : IWriterService
    {
        private readonly IWriterRepository _repo;

        public WriterService(IWriterRepository repo) => _repo = repo;

        public async Task<IEnumerable<WriterDTO>> GetAllAsync()
        {
            var writers = await _repo.GetActiveWritersAsync();
            return writers.Select(MapToViewModel);
        }

        public async Task<WriterDTO?> GetByIdAsync(int id)
        {
            var writer = await _repo.GetByIdAsync(id);
            return writer == null ? null : MapToViewModel(writer);
        }

        public async Task<(bool Success, string Message)> CreateAsync(WriterDTO model)
        {
            var entity = MapToEntity(model);
            await _repo.AddAsync(entity);
            return (true, "writer created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(WriterDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "writer not found.");

            existing.WriterName = model.WriterName;
            
            await _repo.UpdateAsync(existing);
            return (true, "writer updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            if (!await _repo.ExistsAsync(id))
                return (false, "writer not found.");

            await _repo.DeleteAsync(id);
            return (true, "writer deleted successfully.");
        }

        private static WriterDTO MapToViewModel(EditorEntitiesLayer.Entities.Writer w) => new()
        {
            Id = w.Id,
            WriterName = w.WriterName,
        };

        private static EditorEntitiesLayer.Entities.Writer MapToEntity(WriterDTO vm) => new()
        {
            Id = vm.Id,
            WriterName = vm.WriterName,
        };
    }
}
