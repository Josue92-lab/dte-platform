namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class Address : ValueObject
{
    public DepartmentCode Department { get; }
    public MunicipalityCode Municipality { get; }
    public string Complement { get; }

    private Address(DepartmentCode department, MunicipalityCode municipality, string complement)
    {
        Department = department;
        Municipality = municipality;
        Complement = complement;
    }

    public static Result<Address> Create(DepartmentCode department, MunicipalityCode municipality, string complement)
    {
        if (department is null)
        {
            return Result.Failure<Address>(new Error("Address.DepartmentNull", "Department is required."));
        }

        if (municipality is null)
        {
            return Result.Failure<Address>(new Error("Address.MunicipalityNull", "Municipality is required."));
        }

        if (string.IsNullOrWhiteSpace(complement))
        {
            return Result.Failure<Address>(new Error("Address.ComplementEmpty", "Address complement cannot be empty."));
        }

        if (complement.Length > 200)
        {
            return Result.Failure<Address>(new Error("Address.ComplementTooLong", "Address complement must not exceed 200 characters."));
        }

        return Result.Success(new Address(department, municipality, complement));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Department;
        yield return Municipality;
        yield return Complement;
    }
}
