using ControlNumberSeriesAggregate = global::DTE.Domain.Aggregates.ControlNumberSeries.ControlNumberSeries;
using DTE.Domain.Events;
using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.Aggregates.ControlNumberSeries;

public class ControlNumberSeriesTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValid()
    {
        var dteType = DteType.FacturaElectronica;
        var result = ControlNumberSeriesAggregate.Create(dteType, "M001", "P001");

        result.IsSuccess.Should().BeTrue();

        var series = result.Value;
        series.DteType.Should().Be(dteType);
        series.EstablishmentCode.Should().Be("M001");
        series.PosCode.Should().Be("P001");
        series.NextCorrelative.Should().Be(1UL);

        var domainEvent = series.GetDomainEvents().SingleOrDefault() as ControlNumberSeriesCreated;
        domainEvent.Should().NotBeNull();
        domainEvent!.DteType.Should().Be(dteType);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenEstablishmentAndPosCodeLengthIsInvalid()
    {
        var dteType = DteType.FacturaElectronica;
        var result = ControlNumberSeriesAggregate.Create(dteType, "M01", "P01"); // Length is 6

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ControlNumberSeries.InvalidScopeLength");
    }

    [Fact]
    public void AllocateNext_ShouldIncrementCorrelativeAndReturnValidControlNumber()
    {
        var dteType = DteType.FacturaElectronica;
        var series = ControlNumberSeriesAggregate.Create(dteType, "M001", "P001").Value;

        series.ClearDomainEvents(); // Clear creation event

        // First allocation
        var firstAllocation = series.AllocateNext();
        firstAllocation.IsSuccess.Should().BeTrue();
        firstAllocation.Value.Value.Should().Be("DTE-01-M001P001-000000000000001");
        series.NextCorrelative.Should().Be(2UL);

        var firstEvent = series.GetDomainEvents().OfType<ControlNumberAllocated>().SingleOrDefault();
        firstEvent.Should().NotBeNull();
        firstEvent!.ControlNumber.Should().Be(firstAllocation.Value);

        series.ClearDomainEvents();

        // Second allocation
        var secondAllocation = series.AllocateNext();
        secondAllocation.IsSuccess.Should().BeTrue();
        secondAllocation.Value.Value.Should().Be("DTE-01-M001P001-000000000000002");
        series.NextCorrelative.Should().Be(3UL);
    }
}
