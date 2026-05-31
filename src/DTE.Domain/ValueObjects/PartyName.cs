namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class PartyName : ValueObject
{
    public string LegalName { get; }
    public string? CommercialName { get; }

    private PartyName(string legalName, string? commercialName)
    {
        LegalName = legalName;
        CommercialName = commercialName;
    }

    public static Result<PartyName> Create(string legalName, string? commercialName = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result.Failure<PartyName>(new Error("PartyName.LegalNameEmpty", "Legal name cannot be empty."));
        }

        if (legalName.Length > 250)
        {
            return Result.Failure<PartyName>(new Error("PartyName.LegalNameTooLong", "Legal name must not exceed 250 characters."));
        }

        if (commercialName is not null && (string.IsNullOrWhiteSpace(commercialName) || commercialName.Length > 150))
        {
            if (string.IsNullOrWhiteSpace(commercialName))
            {
                commercialName = null;
            }
            else
            {
                return Result.Failure<PartyName>(new Error("PartyName.CommercialNameTooLong", "Commercial name must not exceed 150 characters."));
            }
        }

        return Result.Success(new PartyName(legalName, commercialName));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return LegalName;
        yield return CommercialName ?? string.Empty;
    }
}
