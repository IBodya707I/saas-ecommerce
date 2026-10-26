using Catalog.Bll.Dtos;

namespace Catalog.Bll.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(Guid storeId, CancellationToken ct = default);
    Task<CategoryDto> GetByIdWithSubCategoriesAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryFilterDto>> GetCategoryFiltersAsync(Guid storeId, Guid categoryId, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
