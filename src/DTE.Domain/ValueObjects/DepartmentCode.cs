namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class DepartmentCode : ValueObject
{
    public string Value { get; }

    private DepartmentCode(string value)
    {
        Value = value;
    }

    public static Result<DepartmentCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DepartmentCode>(new Error("DepartmentCode.Empty", "Department code cannot be empty."));
        }

        if (value.Length != 2)
        {
            return Result.Failure<DepartmentCode>(new Error("DepartmentCode.InvalidLength", "Department code must be exactly 2 characters."));
        }

        return Result.Success(new DepartmentCode(value));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
