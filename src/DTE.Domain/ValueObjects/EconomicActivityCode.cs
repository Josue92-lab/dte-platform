namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class EconomicActivityCode : ValueObject
{
    public string Code { get; }
    public string Description { get; }

    private EconomicActivityCode(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public static Result<EconomicActivityCode> Create(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<EconomicActivityCode>(new Error("EconomicActivityCode.CodeEmpty", "Economic activity code cannot be empty."));
        }

        if (code.Length < 5 || code.Length > 6)
        {
            return Result.Failure<EconomicActivityCode>(new Error("EconomicActivityCode.CodeInvalidLength", "Economic activity code must be between 5 and 6 characters."));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<EconomicActivityCode>(new Error("EconomicActivityCode.DescriptionEmpty", "Economic activity description cannot be empty."));
        }

        if (description.Length < 5 || description.Length > 150)
        {
            return Result.Failure<EconomicActivityCode>(new Error("EconomicActivityCode.DescriptionInvalidLength", "Economic activity description must be between 5 and 150 characters."));
        }

        return Result.Success(new EconomicActivityCode(code, description));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Description;
    }
}
