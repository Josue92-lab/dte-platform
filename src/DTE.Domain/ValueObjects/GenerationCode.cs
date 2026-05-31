namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class GenerationCode : ValueObject
{
    public Guid Value { get; }

    private GenerationCode(Guid value)
    {
        Value = value;
    }

    public static Result<GenerationCode> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<GenerationCode>(new Error("GenerationCode.Empty", "Generation code cannot be empty."));
        }

        return Result.Success(new GenerationCode(value));
    }

    public static Result<GenerationCode> Parse(string value)
    {
        if (Guid.TryParse(value, out var guid))
        {
            return Create(guid);
        }

        return Result.Failure<GenerationCode>(new Error("GenerationCode.InvalidFormat", "Generation code is not a valid UUID."));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
