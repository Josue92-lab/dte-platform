using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class QuantityTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsStrictlyPositive()
    {
        var result = Quantity.Create(0.5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(0.5m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1.5)]
    public void Create_ShouldReturnFailure_WhenValueIsZeroOrNegative(decimal value)
    {
        var result = Quantity.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Quantity.ZeroOrNegative");
    }
}
