namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;
using System.Text.RegularExpressions;

public sealed class ControlNumber : ValueObject
{
    private static readonly Regex _formatRegex = new(@"^DTE-\d{2}-[A-Z0-9]{8}-\d{15}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private ControlNumber(string value)
    {
        Value = value;
    }

    public static Result<ControlNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ControlNumber>(new Error("ControlNumber.Empty", "Control number cannot be empty."));
        }

        if (!_formatRegex.IsMatch(value))
        {
            return Result.Failure<ControlNumber>(new Error("ControlNumber.InvalidFormat", "Control number format is invalid."));
        }

        return Result.Success(new ControlNumber(value.ToUpperInvariant()));
    }

    public override string ToString() => Value;

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
