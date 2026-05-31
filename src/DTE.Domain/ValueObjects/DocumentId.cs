namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class DocumentId : ValueObject
{
    public Guid Value { get; }

    private DocumentId(Guid value)
    {
        Value = value;
    }

    public static Result<DocumentId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<DocumentId>(new Error("DocumentId.Empty", "Document ID cannot be empty."));
        }

        return Result.Success(new DocumentId(value));
    }

    public static DocumentId New() => new(Guid.NewGuid());

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
