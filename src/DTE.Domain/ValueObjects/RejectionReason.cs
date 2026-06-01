namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class RejectionReason : ValueObject
{
    public const int MaxDescriptionLength = 500;

    public string Code { get; }
    public string Description { get; }

    private RejectionReason(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public static Result<RejectionReason> Create(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<RejectionReason>(new Error("RejectionReason.CodeEmpty", "Rejection reason code cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<RejectionReason>(new Error("RejectionReason.DescriptionEmpty", "Rejection reason description cannot be empty."));
        }

        if (description.Length > MaxDescriptionLength)
        {
            return Result.Failure<RejectionReason>(new Error("RejectionReason.DescriptionTooLong", $"Rejection reason description must not exceed {MaxDescriptionLength} characters."));
        }

        return Result.Success(new RejectionReason(code, description));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Description;
    }
}
