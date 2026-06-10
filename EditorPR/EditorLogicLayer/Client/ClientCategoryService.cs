using EditorEntitiesLayer.Entities;
using EditorRepositoryLayer.IRepositories;
using EditorViewModelLayer.ClientViewModel;

namespace EditorLogicLayer.Client
{
    public class ClientCategoryService : IClientCategoryService
    {
        private readonly IClientCategoryRepository _repo;

        public ClientCategoryService(IClientCategoryRepository repo)
            => _repo = repo;

        public async Task<IEnumerable<ClientCategoryDTO>> GetByClientAsync(int clientId)
        {
            var list = await _repo.GetByClientAsync(clientId);
            return list.Select(MapToDTO);
        }

        public async Task<IEnumerable<ClientCategoryDTO>> GetRootCategoriesAsync(int clientId)
        {
            var list = await _repo.GetRootCategoriesByClientAsync(clientId);
            return list.Select(MapToDTO);
        }

        public async Task<IEnumerable<ClientCategoryDTO>> GetChildrenAsync(int parentId)
        {
            var list = await _repo.GetChildrenAsync(parentId);
            return list.Select(MapToDTO);
        }

        public async Task<ClientCategoryDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<(bool Success, string Message)> CreateAsync(ClientCategoryDTO model)
        {
            if (await _repo.CategoryNameExistsAsync(model.ClientId, model.CategoryName))
                return (false, "A category with this name already exists for this client.");

            var entity = new ClientCategories
            {
                ClientId = model.ClientId,
                CategoryName = model.CategoryName,
                ParentCategory = model.ParentCategory,
                CategoryType = model.CategoryType,
                Status = "Active",
                Order = model.Order,
                IsActive = true,
                Deleted = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            return (true, "Category created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(ClientCategoryDTO model)
        {
            var existing = await _repo.GetByIdAsync(model.Id);
            if (existing == null)
                return (false, "Category not found.");

            if (model.ParentCategory.HasValue && model.ParentCategory == model.Id)
                return (false, "A category cannot be its own parent.");

            if (await _repo.CategoryNameExistsAsync(model.ClientId, model.CategoryName, model.Id))
                return (false, "Another category with this name already exists for this client.");

            existing.CategoryName = model.CategoryName;
            existing.ParentCategory = model.ParentCategory;
            existing.CategoryType = model.CategoryType;
            existing.Order = model.Order;
            existing.UpdatedAt = DateTime.UtcNow;
            // Status and IsActive are managed exclusively by ToggleStatusAsync — not by Edit

            await _repo.UpdateAsync(existing);
            return (true, "Category updated successfully.");
        }

        // ToggleStatus: flips Active <-> Inactive. Never touches Deleted.
        public async Task<(bool Success, string Message)> ToggleStatusAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Category not found.");

            if (existing.IsActive)
            {
                existing.IsActive = false;
                existing.Status = "Inactive";
                existing.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(existing);
                return (true, $"Category \"{existing.CategoryName}\" has been disabled.");
            }
            else
            {
                existing.IsActive = true;
                existing.Status = "Active";
                existing.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(existing);
                return (true, $"Category \"{existing.CategoryName}\" has been enabled.");
            }
        }
        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return (false, "Category not found.");

            // Soft delete
            existing.Deleted = 1;
            existing.IsActive = false;
            existing.DeletedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, "Category deleted successfully.");
        }
        private static ClientCategoryDTO MapToDTO(ClientCategories c) => new()
        {
            Id = c.Id,
            ClientId = c.ClientId,
            ClientName = c.Client?.Name,
            CategoryName = c.CategoryName,
            ParentCategory = c.ParentCategory,
            ParentCategoryName = c.Parent?.CategoryName,
            CategoryType = c.CategoryType,
            Status = c.Status,
            Order = c.Order,
            SubCategories = c.Children?
                                   .Where(ch => ch.Deleted == 0)   // show all non-deleted children (active + inactive)
                                   .Select(MapToDTO)
                                 ?? Enumerable.Empty<ClientCategoryDTO>()
        };
    }
}