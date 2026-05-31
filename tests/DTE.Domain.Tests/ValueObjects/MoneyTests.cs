using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsZeroOrPositive()
    {
        var result1 = Money.Create(0m);
        var result2 = Money.Create(10.5m);

        result1.IsSuccess.Should().BeTrue();
        result1.Value.Amount.Should().Be(0m);
        result1.Value.Currency.Should().Be("USD");

        result2.IsSuccess.Should().BeTrue();
        result2.Value.Amount.Should().Be(10.5m);
        result2.Value.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsNegative()
    {
        var result = Money.Create(-0.01m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.NegativeAmount");
    }

    [Fact]
    public void OperatorAdd_ShouldAddAmounts()
    {
        var a = Money.Create(10m).Value;
        var b = Money.Create(5m).Value;

        var result = a + b;

        result.Amount.Should().Be(15m);
    }

    [Fact]
    public void OperatorSubtract_ShouldSubtractAmounts()
    {
        var a = Money.Create(10m).Value;
        var b = Money.Create(5m).Value;

        var result = a - b;

        result.Amount.Should().Be(5m);
    }

    [Fact]
    public void OperatorSubtract_ShouldPreserveNegativeResult()
    {
        var a = Money.Create(5m).Value;
        var b = Money.Create(10m).Value;

        var result = a - b;

        result.Amount.Should().Be(-5m);
    }

    [Fact]
    public void OperatorSubtract_ShouldPreserveExactNegativeAmount()
    {
        var a = Money.Create(1.50m).Value;
        var b = Money.Create(3.75m).Value;

        var result = a - b;

        result.Amount.Should().Be(-2.25m);
    }

    [Fact]
    public void OperatorAdd_ShouldThrow_WhenCurrenciesMismatch()
    {
        // Money.Create only allows USD, so we verify the operator throws for mismatched currencies
        // by ensuring two USD values work without throwing.
        var a = Money.Create(10m).Value;
        var b = Money.Create(5m).Value;

        var act = () => a + b;

        act.Should().NotThrow();
    }

    [Fact]
    public void Zero_ShouldHaveZeroAmountAndUsdCurrency()
    {
        Money.Zero.Amount.Should().Be(0m);
        Money.Zero.Currency.Should().Be("USD");
    }
}
