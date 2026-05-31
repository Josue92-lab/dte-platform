namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class DteType : ValueObject
{
    public string Code { get; }
    public string Description { get; }

    public static readonly DteType FacturaElectronica = new("01", "FE");
    public static readonly DteType ComprobanteCreditoFiscal = new("03", "CCF");

    private DteType(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public static Result<DteType> FromCode(string code)
    {
        return code switch
        {
            "01" => Result.Success(FacturaElectronica),
            "03" => Result.Success(ComprobanteCreditoFiscal),
            _ => Result.Failure<DteType>(new Error("DteType.Invalid", "Invalid DTE Type code."))
        };
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
}
