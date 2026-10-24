using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Orders.Bll.Dtos.Customers;
using Orders.Bll.Interfaces;
using Orders.Dal.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.Bll.Services;

/// <summary>
/// Implementation of the customer service business logic.
/// </summary>
public sealed class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, Guid storeId, CancellationToken ct = default)
    {
        var customer = await _uow.Customers.GetByIdAsync(id, storeId, ct);
        return customer is null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto?> GetByPhoneAsync(string phone, Guid storeId, CancellationToken ct = default)
    {
        var customer = await _uow.Customers.GetByPhoneAsync(phone, storeId, ct);
        return customer is null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        var customers = await _uow.Customers.GetAllByStoreAsync(storeId, ct);
        return _mapper.Map<IReadOnlyList<CustomerDto>>(customers);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        var existing = await _uow.Customers.GetByPhoneAsync(dto.Phone, dto.StoreId, ct);
        if (existing is not null)
        {
            throw new BusinessConflictException($"Customer with phone {dto.Phone} already exists in this store");
        }

        var customer = _mapper.Map<Customer>(dto);
        customer.Id = Guid.NewGuid();
        customer.IsRegistered = !string.IsNullOrWhiteSpace(dto.Password);
        customer.PasswordHash = string.IsNullOrWhiteSpace(dto.Password) ? null : HashPassword(dto.Password);

        var createdId = await _uow.Customers.AddAsync(customer, ct);
        var created = await _uow.Customers.GetByIdAsync(createdId, dto.StoreId, ct)
            ?? throw new NotFoundException($"Customer {createdId} was not found after creation");

        return _mapper.Map<CustomerDto>(created);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid storeId, CancellationToken ct = default)
    {
        var existing = await _uow.Customers.GetByIdAsync(id, storeId, ct)
            ?? throw new NotFoundException($"Customer {id} not found");

        return await _uow.Customers.DeleteAsync(id, storeId, ct);
    }

    /// <summary>
    /// Industry-standard password hashing using PBKDF2 with SHA-256 and cryptographic salt.
    /// </summary>
    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
