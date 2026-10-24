using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Orders.Dal.Common;
using Orders.Dal.Interfaces;
using Orders.Domain.Entities;

namespace Orders.Dal.Repositories;

/// <summary>
/// Customer repository implemented using pure ADO.NET (Npgsql).
/// Demonstrates low-level connection management, command creation, parameterization, and stored procedure calls.
/// Deliberately does NOT inherit Generic Repository per lab requirements.
/// </summary>
public sealed class CustomerRepository : RepositoryBase, ICustomerRepository
{
    public CustomerRepository(NpgsqlConnection connection, Func<NpgsqlTransaction?> currentTransaction)
        : base(connection, currentTransaction)
    {
    }

    public async Task<Customer?> GetByIdAsync(Guid id, Guid storeId, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            const string sql = @"
                SELECT id, store_id, phone, email, full_name, password_hash, 
                       is_registered, version, created_at, created_by, updated_at, updated_by, is_deleted
                FROM customers
                WHERE id = @Id AND store_id = @StoreId AND is_deleted = false;";

            await EnsureOpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, Connection, Transaction);
            cmd.Parameters.Add(new NpgsqlParameter("Id", NpgsqlDbType.Uuid) { Value = id });
            cmd.Parameters.Add(new NpgsqlParameter("StoreId", NpgsqlDbType.Uuid) { Value = storeId });

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            return MapFromReader(reader);
        });
    }

    public async Task<Customer?> GetByPhoneAsync(string phone, Guid storeId, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            const string sql = @"
                SELECT id, store_id, phone, email, full_name, password_hash, 
                       is_registered, version, created_at, created_by, updated_at, updated_by, is_deleted
                FROM customers
                WHERE phone = @Phone AND store_id = @StoreId AND is_deleted = false;";

            await EnsureOpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, Connection, Transaction);
            cmd.Parameters.Add(new NpgsqlParameter("Phone", NpgsqlDbType.Varchar) { Value = phone });
            cmd.Parameters.Add(new NpgsqlParameter("StoreId", NpgsqlDbType.Uuid) { Value = storeId });

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            return MapFromReader(reader);
        });
    }

    public async Task<IReadOnlyList<Customer>> GetAllByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            const string sql = @"
                SELECT id, store_id, phone, email, full_name, password_hash, 
                       is_registered, version, created_at, created_by, updated_at, updated_by, is_deleted
                FROM customers
                WHERE store_id = @StoreId AND is_deleted = false
                ORDER BY created_at DESC;";

            await EnsureOpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, Connection, Transaction);
            cmd.Parameters.Add(new NpgsqlParameter("StoreId", NpgsqlDbType.Uuid) { Value = storeId });

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var list = new List<Customer>();
            while (await reader.ReadAsync(ct))
            {
                list.Add(MapFromReader(reader));
            }

            return list;
        });
    }

    public async Task<Guid> AddAsync(Customer customer, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            if (customer.Id == Guid.Empty)
            {
                customer.Id = Guid.NewGuid();
            }

            // Invoking usp_create_customer stored procedure
            const string sql = @"
                CALL usp_create_customer(@Id, @StoreId, @Phone, @Email, @FullName, @PasswordHash, @IsRegistered);";

            await EnsureOpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, Connection, Transaction);
            cmd.Parameters.Add(new NpgsqlParameter("Id", NpgsqlDbType.Uuid) { Value = customer.Id });
            cmd.Parameters.Add(new NpgsqlParameter("StoreId", NpgsqlDbType.Uuid) { Value = customer.StoreId });
            cmd.Parameters.Add(new NpgsqlParameter("Phone", NpgsqlDbType.Varchar) { Value = customer.Phone });
            cmd.Parameters.Add(new NpgsqlParameter("Email", NpgsqlDbType.Varchar) { Value = (object?)customer.Email ?? DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter("FullName", NpgsqlDbType.Varchar) { Value = customer.FullName });
            cmd.Parameters.Add(new NpgsqlParameter("PasswordHash", NpgsqlDbType.Varchar) { Value = (object?)customer.PasswordHash ?? DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter("IsRegistered", NpgsqlDbType.Boolean) { Value = customer.IsRegistered });

            await cmd.ExecuteNonQueryAsync(ct);
            return customer.Id;
        });
    }

    public async Task UpdateAsync(Customer customer, int expectedVersion, CancellationToken ct = default)
    {
        await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            // Invoking usp_update_customer with optimistic concurrency check
            const string sql = @"
                CALL usp_update_customer(@Id, @StoreId, @ExpectedVer, @Phone, @Email, @FullName);";

            await EnsureOpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, Connection, Transaction);
            cmd.Parameters.Add(new NpgsqlParameter("Id", NpgsqlDbType.Uuid) { Value = customer.Id });
            cmd.Parameters.Add(new NpgsqlParameter("StoreId", NpgsqlDbType.Uuid) { Value = customer.StoreId });
            cmd.Parameters.Add(new NpgsqlParameter("ExpectedVer", NpgsqlDbType.Integer) { Value = expectedVersion });
            cmd.Parameters.Add(new NpgsqlParameter("Phone", NpgsqlDbType.Varchar) { Value = customer.Phone });
            cmd.Parameters.Add(new NpgsqlParameter("Email", NpgsqlDbType.Varchar) { Value = (object?)customer.Email ?? DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter("FullName", NpgsqlDbType.Varchar) { Value = customer.FullName });

            await cmd.ExecuteNonQueryAsync(ct);
        });
    }

    public async Task<bool> DeleteAsync(Guid id, Guid storeId, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            const string sql = @"
                UPDATE customers
                SET is_deleted = true,
                    updated_at = CURRENT_TIMESTAMP,
                    updated_by = 'system_delete'
                WHERE id = @Id AND store_id = @StoreId AND is_deleted = false;";

            await EnsureOpenAsync(ct);
            await using var cmd = new NpgsqlCommand(sql, Connection, Transaction);
            cmd.Parameters.Add(new NpgsqlParameter("Id", NpgsqlDbType.Uuid) { Value = id });
            cmd.Parameters.Add(new NpgsqlParameter("StoreId", NpgsqlDbType.Uuid) { Value = storeId });

            var affected = await cmd.ExecuteNonQueryAsync(ct);
            return affected > 0;
        });
    }

    private static Customer MapFromReader(NpgsqlDataReader reader)
    {
        return new Customer
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            StoreId = reader.GetGuid(reader.GetOrdinal("store_id")),
            Phone = reader.GetString(reader.GetOrdinal("phone")),
            Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
            FullName = reader.GetString(reader.GetOrdinal("full_name")),
            PasswordHash = reader.IsDBNull(reader.GetOrdinal("password_hash")) ? null : reader.GetString(reader.GetOrdinal("password_hash")),
            IsRegistered = reader.GetBoolean(reader.GetOrdinal("is_registered")),
            Version = reader.GetInt32(reader.GetOrdinal("version")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime(reader.GetOrdinal("updated_at")),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetString(reader.GetOrdinal("updated_by")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted"))
        };
    }
}
