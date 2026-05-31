namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class Quantity : ValueObject
{
    public decimal Value { get; }

    private Quantity(decimal value)
    {
        Value = value;
    }

    public static Result<Quantity> Create(decimal value)
    {
        if (value <= 0)
        {
            return Result.Failure<Quantity>(new Error("Quantity.ZeroOrNegative", "Quantity must be strictly greater than zero."));
        }

        return Result.Success(new Quantity(value));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
