namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;
using System.Text.RegularExpressions;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex _phoneRegex = new(@"^\+?[0-9\s\-]{6,20}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PhoneNumber>(new Error("PhoneNumber.Empty", "Phone number cannot be empty."));
        }

        if (!_phoneRegex.IsMatch(value))
        {
            return Result.Failure<PhoneNumber>(new Error("PhoneNumber.InvalidFormat", "Phone number format is invalid."));
        }

        return Result.Success(new PhoneNumber(value));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
