using System;
using System.Threading.Tasks;
using Npgsql;
using Orders.Domain.Exceptions;

namespace Orders.Dal.Common;

/// <summary>
/// Maps PostgreSQL technical exceptions (PostgresException) and custom procedure error codes
/// to domain exceptions (NotFoundException, BusinessConflictException, ValidationException).
/// </summary>
public static class PostgresExceptionMapper
{
    public static Exception Map(PostgresException ex)
    {
        return ex.SqlState switch
        {
            // ORD02: Entity not found (customer, order, product)
            "ORD02" => new NotFoundException(ex.MessageText),

            // ORD00: Empty cart; ORD03: Invalid format (phone, provider); 23514: check_violation
            "ORD00" or "ORD03" or "23514" => new ValidationException(ex.MessageText),

            // ORD01: Invalid State Machine transition
            // ORD04: Attempting to ship unpaid prepaid order
            // STK01: Insufficient stock
            // OPT01: Optimistic concurrency conflict
            "ORD01" or "ORD04" or "STK01" or "OPT01" => new BusinessConflictException(ex.MessageText),

            // 23505: unique_violation in PostgreSQL
            "23505" => new BusinessConflictException($"Record with the same unique key already exists: {ex.Detail ?? ex.MessageText}"),

            // 23503: foreign_key_violation in PostgreSQL
            "23503" => new BusinessConflictException($"Referenced record not found or dependent records exist: {ex.Detail ?? ex.MessageText}"),

            _ => ex
        };
    }

    public static async Task<T> ExecuteWithMappingAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (PostgresException ex)
        {
            throw Map(ex);
        }
    }

    public static async Task ExecuteWithMappingAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (PostgresException ex)
        {
            throw Map(ex);
        }
    }
}
