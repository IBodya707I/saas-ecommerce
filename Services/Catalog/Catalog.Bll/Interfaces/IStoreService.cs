using Catalog.Bll.Dtos;

namespace Catalog.Bll.Interfaces;

public interface IStoreService
{
    Task<StoreDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StoreEmployeeDto>> GetEmployeesAsync(Guid storeId, CancellationToken ct = default);
    Task<StoreDto> CreateAsync(CreateStoreDto dto, CancellationToken ct = default);
    Task<StoreEmployeeDto> AssignEmployeeAsync(AssignStoreEmployeeDto dto, CancellationToken ct = default);
}
