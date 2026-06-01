namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class DocumentSignature : ValueObject
{
    public string Value { get; }

    private DocumentSignature(string value)
    {
        Value = value;
    }

    public static Result<DocumentSignature> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DocumentSignature>(new Error("DocumentSignature.Empty", "Document signature cannot be empty."));
        }

        return Result.Success(new DocumentSignature(value));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
