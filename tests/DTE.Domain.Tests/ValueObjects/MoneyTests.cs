using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenAmountIsPositiveAndCurrencyIsUSD()
    {
        var result = Money.Create(100.50m, "USD");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenAmountIsNegative()
    {
        var result = Money.Create(-10m, "USD");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCurrencyIsNotUSD()
    {
        var result = Money.Create(10m, "EUR");
        result.IsFailure.Should().BeTrue();
    }
}
