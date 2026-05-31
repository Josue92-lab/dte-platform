namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;
using System.Text.RegularExpressions;

public sealed class EmailAddress : ValueObject
{
    private static readonly Regex _emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static Result<EmailAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<EmailAddress>(new Error("EmailAddress.Empty", "Email address cannot be empty."));
        }

        if (!_emailRegex.IsMatch(value))
        {
            return Result.Failure<EmailAddress>(new Error("EmailAddress.InvalidFormat", "Email address format is invalid."));
        }

        return Result.Success(new EmailAddress(value.ToLowerInvariant()));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
