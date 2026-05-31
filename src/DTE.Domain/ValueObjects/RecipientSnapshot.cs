namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class RecipientSnapshot : ValueObject
{
    public PartyName Name { get; }
    public TaxIdentifier? DocumentIdentifier { get; }
    public TaxIdentifier? Nrc { get; }
    public EconomicActivityCode? EconomicActivity { get; }
    public Address? Address { get; }
    public PhoneNumber? Phone { get; }
    public EmailAddress? Email { get; }

    private RecipientSnapshot(
        PartyName name,
        TaxIdentifier? documentIdentifier,
        TaxIdentifier? nrc,
        EconomicActivityCode? economicActivity,
        Address? address,
        PhoneNumber? phone,
        EmailAddress? email)
    {
        Name = name;
        DocumentIdentifier = documentIdentifier;
        Nrc = nrc;
        EconomicActivity = economicActivity;
        Address = address;
        Phone = phone;
        Email = email;
    }

    public static Result<RecipientSnapshot> Create(
        PartyName name,
        TaxIdentifier? documentIdentifier = null,
        TaxIdentifier? nrc = null,
        EconomicActivityCode? economicActivity = null,
        Address? address = null,
        PhoneNumber? phone = null,
        EmailAddress? email = null)
    {
        if (name is null)
        {
            return Result.Failure<RecipientSnapshot>(new Error("RecipientSnapshot.NameNull", "Recipient name is required."));
        }

        return Result.Success(new RecipientSnapshot(name, documentIdentifier, nrc, economicActivity, address, phone, email));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Name;
        yield return DocumentIdentifier ?? (object)string.Empty;
        yield return Nrc ?? (object)string.Empty;
        yield return EconomicActivity ?? (object)string.Empty;
        yield return Address ?? (object)string.Empty;
        yield return Phone ?? (object)string.Empty;
        yield return Email ?? (object)string.Empty;
    }
}
