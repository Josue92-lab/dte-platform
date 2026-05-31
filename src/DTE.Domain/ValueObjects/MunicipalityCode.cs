namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class MunicipalityCode : ValueObject
{
    public string Value { get; }

    private MunicipalityCode(string value)
    {
        Value = value;
    }

    public static Result<MunicipalityCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<MunicipalityCode>(new Error("MunicipalityCode.Empty", "Municipality code cannot be empty."));
        }

        if (value.Length != 2)
        {
            return Result.Failure<MunicipalityCode>(new Error("MunicipalityCode.InvalidLength", "Municipality code must be exactly 2 characters."));
        }

        return Result.Success(new MunicipalityCode(value));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
