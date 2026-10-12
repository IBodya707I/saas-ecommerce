using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Orders.Dal.Common;
using Orders.Dal.Interfaces;

namespace Orders.Dal.Repositories;

/// <summary>
/// Generic repository base class powered by Dapper.
/// Implements standard CRUD operations for domain entities.
/// </summary>
public abstract class BaseDapperRepository<T> : RepositoryBase, IGenericRepository<T> where T : class
{
    static BaseDapperRepository()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    protected readonly string TableName;

    protected BaseDapperRepository(NpgsqlConnection connection, Func<NpgsqlTransaction?> currentTransaction, string tableName)
        : base(connection, currentTransaction)
    {
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            var sql = $"SELECT * FROM {TableName} WHERE id = @Id AND is_deleted = false;";
            var cmd = new CommandDefinition(sql, new { Id = id }, Transaction, cancellationToken: ct);
            return await Connection.QuerySingleOrDefaultAsync<T>(cmd);
        });
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            var sql = $"SELECT * FROM {TableName} WHERE is_deleted = false;";
            var cmd = new CommandDefinition(sql, transaction: Transaction, cancellationToken: ct);
            var rows = await Connection.QueryAsync<T>(cmd);
            return rows.AsList();
        });
    }

    public abstract Task<Guid> AddAsync(T entity, CancellationToken ct = default);

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            await EnsureOpenAsync(ct);
            var sql = $"UPDATE {TableName} SET is_deleted = true, updated_at = CURRENT_TIMESTAMP WHERE id = @Id AND is_deleted = false;";
            var cmd = new CommandDefinition(sql, new { Id = id }, Transaction, cancellationToken: ct);
            var affected = await Connection.ExecuteAsync(cmd);
            return affected > 0;
        });
    }
}
