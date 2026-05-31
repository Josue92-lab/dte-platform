namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class IssuerSnapshot : ValueObject
{
    public TaxIdentifier Nit { get; }
    public TaxIdentifier Nrc { get; }
    public PartyName Name { get; }
    public EconomicActivityCode EconomicActivity { get; }
    public Address Address { get; }
    public PhoneNumber Phone { get; }
    public EmailAddress Email { get; }

    private IssuerSnapshot(
        TaxIdentifier nit,
        TaxIdentifier nrc,
        PartyName name,
        EconomicActivityCode economicActivity,
        Address address,
        PhoneNumber phone,
        EmailAddress email)
    {
        Nit = nit;
        Nrc = nrc;
        Name = name;
        EconomicActivity = economicActivity;
        Address = address;
        Phone = phone;
        Email = email;
    }

    public static Result<IssuerSnapshot> Create(
        TaxIdentifier nit,
        TaxIdentifier nrc,
        PartyName name,
        EconomicActivityCode economicActivity,
        Address address,
        PhoneNumber phone,
        EmailAddress email)
    {
        if (nit is null)
        {
            return Result.Failure<IssuerSnapshot>(new Error("IssuerSnapshot.NitNull", "Issuer NIT is required."));
        }

        if (nrc is null)
        {
            return Result.Failure<IssuerSnapshot>(new Error("IssuerSnapshot.NrcNull", "Issuer NRC is required."));
        }

        if (name is null)
        {
            return Result.Failure<IssuerSnapshot>(new Error("IssuerSnapshot.NameNull", "Issuer name is required."));
        }

        if (economicActivity is null)
        {
            return Result.Failure<IssuerSnapshot>(new Error("IssuerSnapshot.EconomicActivityNull", "Issuer economic activity is required."));
        }

        if (address is null)
        {
            return Result.Failure<IssuerSnapshot>(new Error("IssuerSnapshot.AddressNull", "Issuer address is required."));
        }

        if (phone is null)
        {
            return Result.Failure<IssuerSnapshot>(new Error("IssuerSnapshot.PhoneNull", "Issuer phone is required."));
        }

        if (email is null)
        {
            return Result.Failure<IssuerSnapshot>(new Error("IssuerSnapshot.EmailNull", "Issuer email is required."));
        }

        return Result.Success(new IssuerSnapshot(nit, nrc, name, economicActivity, address, phone, email));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Nit;
        yield return Nrc;
        yield return Name;
        yield return EconomicActivity;
        yield return Address;
        yield return Phone;
        yield return Email;
    }
}
