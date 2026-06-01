namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class ReceptionStamp : ValueObject
{
    public string Value { get; }

    private ReceptionStamp(string value)
    {
        Value = value;
    }

    public static Result<ReceptionStamp> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ReceptionStamp>(new Error("ReceptionStamp.Empty", "Reception stamp cannot be empty."));
        }

        return Result.Success(new ReceptionStamp(value));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
