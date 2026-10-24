using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Orders.Dal.Common;

/// <summary>
/// Base class for all repositories.
/// Provides access to the shared NpgsqlConnection and the active transaction from UnitOfWork.
/// </summary>
public abstract class RepositoryBase
{
    protected readonly NpgsqlConnection Connection;
    private readonly Func<NpgsqlTransaction?> _currentTransaction;

    protected RepositoryBase(NpgsqlConnection connection, Func<NpgsqlTransaction?> currentTransaction)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _currentTransaction = currentTransaction ?? throw new ArgumentNullException(nameof(currentTransaction));
    }

    /// <summary>
    /// Current Unit of Work transaction (or null if running outside an explicit transaction).
    /// </summary>
    protected NpgsqlTransaction? Transaction => _currentTransaction();

    /// <summary>
    /// Ensures the connection is opened asynchronously if currently closed.
    /// </summary>
    protected async Task EnsureOpenAsync(CancellationToken ct)
    {
        if (Connection.State != ConnectionState.Open)
        {
            await Connection.OpenAsync(ct);
        }
    }
}
