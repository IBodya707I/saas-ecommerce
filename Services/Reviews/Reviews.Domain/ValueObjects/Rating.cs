namespace Reviews.Domain.ValueObjects;

using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;

public sealed class Rating : ValueObject
{
    public int Value { get; }

    public Rating(int value)
    {
        if (value < 1 || value > 5)
            throw new DomainException("Rating must be between 1 and 5.");

        Value = value;
    }

    public static implicit operator int(Rating rating) => rating.Value;
    public static explicit operator Rating(int value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
