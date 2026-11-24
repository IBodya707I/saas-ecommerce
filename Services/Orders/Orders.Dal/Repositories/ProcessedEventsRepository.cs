using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Orders.Dal.Common;
using Orders.Dal.Interfaces;

namespace Orders.Dal.Repositories;

public sealed class ProcessedEventsRepository : IProcessedEventsRepository
{
    private readonly NpgsqlConnection _connection;
    private readonly Func<NpgsqlTransaction?> _transactionProvider;

    public ProcessedEventsRepository(NpgsqlConnection connection, Func<NpgsqlTransaction?> transactionProvider)
    {
        _connection = connection;
        _transactionProvider = transactionProvider;
    }

    public async Task<bool> TryAddAsync(Guid eventId, CancellationToken ct = default)
    {
        return await PostgresExceptionMapper.ExecuteWithMappingAsync(async () =>
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                await _connection.OpenAsync(ct);
            }

            const string ensureTableSql = @"
                CREATE TABLE IF NOT EXISTS processed_events (
                    event_id UUID PRIMARY KEY,
                    processed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";

            var ensureCmd = new CommandDefinition(ensureTableSql, null, _transactionProvider(), cancellationToken: ct);
            await _connection.ExecuteAsync(ensureCmd);

            const string insertSql = @"
                INSERT INTO processed_events (event_id, processed_at)
                VALUES (@EventId, CURRENT_TIMESTAMP)
                ON CONFLICT (event_id) DO NOTHING;";

            var cmd = new CommandDefinition(
                insertSql,
                new { EventId = eventId },
                _transactionProvider(),
                cancellationToken: ct);

            var affected = await _connection.ExecuteAsync(cmd);
            return affected > 0;
        });
    }
}
