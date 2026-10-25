using Catalog.Bll.Dtos;
using Catalog.Bll.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ValidationException = Catalog.Domain.Exceptions.ValidationException;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly IValidator<CreateStoreDto> _createValidator;

    public StoresController(
        IStoreService storeService,
        IValidator<CreateStoreDto> createValidator)
    {
        _storeService = storeService;
        _createValidator = createValidator;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StoreDto>> GetById(Guid id, CancellationToken ct)
    {
        var store = await _storeService.GetByIdAsync(id, ct);
        return Ok(store);
    }

    [HttpGet("{id:guid}/employees")]
    public async Task<ActionResult<IReadOnlyList<StoreEmployeeDto>>> GetEmployees(Guid id, CancellationToken ct)
    {
        var employees = await _storeService.GetEmployeesAsync(id, ct);
        return Ok(employees);
    }

    [HttpPost]
    public async Task<ActionResult<StoreDto>> Create([FromBody] CreateStoreDto dto, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ValidationException("Store validation failed", errors);
        }

        var created = await _storeService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/employees")]
    public async Task<ActionResult<StoreEmployeeDto>> AddEmployee(
        Guid id,
        [FromBody] AssignStoreEmployeeDto dto,
        CancellationToken ct)
    {
        if (dto.StoreId == Guid.Empty)
        {
            dto = dto with { StoreId = id };
        }
        else if (dto.StoreId != id)
        {
            throw new ValidationException("Route storeId does not match request body StoreId");
        }

        var employee = await _storeService.AssignEmployeeAsync(dto, ct);
        return Ok(employee);
    }
}
