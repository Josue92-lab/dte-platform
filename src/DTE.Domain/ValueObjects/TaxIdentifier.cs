namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class TaxIdentifier : ValueObject
{
    public string Kind { get; }
    public string Value { get; }

    private TaxIdentifier(string kind, string value)
    {
        Kind = kind;
        Value = value;
    }

    public static Result<TaxIdentifier> Create(string kind, string value)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return Result.Failure<TaxIdentifier>(new Error("TaxIdentifier.KindEmpty", "Tax identifier kind cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<TaxIdentifier>(new Error("TaxIdentifier.ValueEmpty", "Tax identifier value cannot be empty."));
        }

        return Result.Success(new TaxIdentifier(kind.ToUpperInvariant(), value));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Kind;
        yield return Value;
    }
}
