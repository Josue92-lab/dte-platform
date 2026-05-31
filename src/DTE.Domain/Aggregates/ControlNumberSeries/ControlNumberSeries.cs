namespace DTE.Domain.Aggregates.ControlNumberSeries;

using DTE.Domain.Primitives;
using DTE.Domain.ValueObjects;
using DTE.Domain.Events;

public sealed class ControlNumberSeries : AggregateRoot
{
    public DteType DteType { get; private set; }
    public string EstablishmentCode { get; private set; }
    public string PosCode { get; private set; }
    public ulong NextCorrelative { get; private set; }

    private ControlNumberSeries(Guid id, DteType dteType, string establishmentCode, string posCode, ulong nextCorrelative)
        : base(id)
    {
        DteType = dteType;
        EstablishmentCode = establishmentCode;
        PosCode = posCode;
        NextCorrelative = nextCorrelative;
    }

    public static Result<ControlNumberSeries> Create(DteType dteType, string establishmentCode, string posCode)
    {
        if (dteType is null)
        {
            return Result.Failure<ControlNumberSeries>(new Error("ControlNumberSeries.DteTypeNull", "DteType is required."));
        }

        if (string.IsNullOrWhiteSpace(establishmentCode))
        {
            return Result.Failure<ControlNumberSeries>(new Error("ControlNumberSeries.EstablishmentCodeEmpty", "EstablishmentCode is required."));
        }

        if (string.IsNullOrWhiteSpace(posCode))
        {
            return Result.Failure<ControlNumberSeries>(new Error("ControlNumberSeries.PosCodeEmpty", "PosCode is required."));
        }

        // Structural validation based on DTE format rules: MH requires exactly 8 combined chars for Issuing Point
        if (establishmentCode.Length + posCode.Length != 8)
        {
            return Result.Failure<ControlNumberSeries>(new Error("ControlNumberSeries.InvalidScopeLength", "EstablishmentCode and PosCode combined must be exactly 8 characters."));
        }

        var series = new ControlNumberSeries(Guid.NewGuid(), dteType, establishmentCode, posCode, 1);

        series.RaiseDomainEvent(new ControlNumberSeriesCreated(Guid.NewGuid(), DateTime.UtcNow, series.Id, dteType));

        return Result.Success(series);
    }

    public Result<ControlNumber> AllocateNext()
    {
        var correlativeToAllocate = NextCorrelative;
        var formattedCorrelative = correlativeToAllocate.ToString("D15", System.Globalization.CultureInfo.InvariantCulture);

        // Example: DTE-01-M001P001-000000000000001
        var controlNumberValue = $"DTE-{DteType.Code}-{EstablishmentCode}{PosCode}-{formattedCorrelative}";

        var controlNumberResult = ControlNumber.Create(controlNumberValue);
        if (controlNumberResult.IsFailure)
        {
            return Result.Failure<ControlNumber>(controlNumberResult.Error);
        }

        NextCorrelative++;

        RaiseDomainEvent(new ControlNumberAllocated(Guid.NewGuid(), DateTime.UtcNow, Id, controlNumberResult.Value));

        return controlNumberResult;
    }
}
