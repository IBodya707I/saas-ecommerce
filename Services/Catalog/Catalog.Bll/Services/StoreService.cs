using AutoMapper;
using Catalog.Bll.Dtos;
using Catalog.Bll.Interfaces;
using Catalog.Dal;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Bll.Services;

public class StoreService : IStoreService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StoreService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<StoreDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var store = await _uow.Stores.GetByIdAsync(id, ct);
        if (store == null || store.IsDeleted)
            throw new NotFoundException($"Store with ID '{id}' was not found");

        return _mapper.Map<StoreDto>(store);
    }

    public async Task<IReadOnlyList<StoreEmployeeDto>> GetEmployeesAsync(
        Guid storeId,
        string? role = null,
        DateTime? assignedSince = null,
        CancellationToken ct = default)
    {
        _uow.SetStoreContext(storeId);
        var employees = await _uow.StoreEmployees.GetActiveStoreEmployeesWithUsersAsync(storeId, role, assignedSince, ct);
        return _mapper.Map<IReadOnlyList<StoreEmployeeDto>>(employees);
    }

    public async Task<StoreDto> CreateAsync(CreateStoreDto dto, CancellationToken ct = default)
    {
        var existingStores = await _uow.Stores.GetAllAsync(ct);
        if (existingStores.Any(s => string.Equals(s.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase) && !s.IsDeleted))
            throw new ConflictException($"Store with slug '{dto.Slug}' already exists");

        var store = new Store(
            Guid.NewGuid(),
            dto.Name,
            dto.Slug,
            dto.OwnerId,
            dto.Category
        );

        await _uow.Stores.AddAsync(store, ct);

        var employee = new StoreEmployee(store.Id, dto.OwnerId, "Owner");
        await _uow.StoreEmployees.AddAsync(employee, ct);

        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<StoreDto>(store);
    }

    public async Task<StoreEmployeeDto> AssignEmployeeAsync(AssignStoreEmployeeDto dto, CancellationToken ct = default)
    {
        _uow.SetStoreContext(dto.StoreId);

        var store = await _uow.Stores.GetByIdAsync(dto.StoreId, ct);
        if (store == null || store.IsDeleted)
            throw new NotFoundException($"Store with ID '{dto.StoreId}' was not found");

        var user = await _uow.Users.GetByIdAsync(dto.UserId, ct);
        if (user == null || user.IsDeleted)
            throw new NotFoundException($"User with ID '{dto.UserId}' was not found");

        var existingEmployees = await _uow.StoreEmployees.GetActiveStoreEmployeesWithUsersAsync(dto.StoreId, ct: ct);
        if (existingEmployees.Any(e => e.UserId == dto.UserId))
            throw new ConflictException($"User '{dto.UserId}' is already assigned to this store");

        var employee = new StoreEmployee(dto.StoreId, dto.UserId, dto.Role);
        await _uow.StoreEmployees.AddAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<StoreEmployeeDto>(employee);
    }
}
