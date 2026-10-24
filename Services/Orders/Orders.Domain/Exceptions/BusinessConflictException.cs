using System;

namespace Orders.Domain.Exceptions;

public class BusinessConflictException : Exception
{
    public BusinessConflictException(string message) : base(message)
    {
    }

    public BusinessConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
