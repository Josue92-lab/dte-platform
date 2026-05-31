namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class SchemaVersion : ValueObject
{
    public string Value { get; }

    private SchemaVersion(string value)
    {
        Value = value;
    }

    public static Result<SchemaVersion> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<SchemaVersion>(new Error("SchemaVersion.Empty", "Schema version cannot be empty."));
        }

        return Result.Success(new SchemaVersion(value));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
