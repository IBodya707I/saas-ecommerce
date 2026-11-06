namespace Reviews.Domain.ValueObjects;

using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;

public sealed class DeliveryRating : ValueObject
{
    public int Value { get; }

    public DeliveryRating(int value)
    {
        if (value < 1 || value > 5)
            throw new DomainException("Delivery rating must be between 1 and 5.");

        Value = value;
    }

    public static implicit operator int(DeliveryRating rating) => rating.Value;
    public static explicit operator DeliveryRating(int value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
