using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class DocumentTotalsTests
{
    [Fact]
    public void Create_ShouldCalculateSubTotalSalesAndTotalToPayCorrectly()
    {
        var nonTaxable = Money.Create(10m).Value;
        var exempt = Money.Create(20m).Value;
        var taxable = Money.Create(100m).Value;
        var discount = Money.Create(5m).Value;
        var tax = Money.Create(13m).Value;

        var totals = DocumentTotals.Create(nonTaxable, exempt, taxable, discount, tax);

        totals.SubTotalSales.Amount.Should().Be(130m); // 10 + 20 + 100
        totals.TotalToPay.Amount.Should().Be(138m);    // 130 + 13 - 5

        totals.TotalNonTaxable.Should().Be(nonTaxable);
        totals.TotalExempt.Should().Be(exempt);
        totals.TotalTaxable.Should().Be(taxable);
        totals.TotalDiscount.Should().Be(discount);
        totals.TotalTax.Should().Be(tax);
    }

    [Fact]
    public void Zero_ShouldHaveAllZeroAmounts()
    {
        var zero = DocumentTotals.Zero;

        zero.TotalNonTaxable.Amount.Should().Be(0m);
        zero.TotalExempt.Amount.Should().Be(0m);
        zero.TotalTaxable.Amount.Should().Be(0m);
        zero.SubTotalSales.Amount.Should().Be(0m);
        zero.TotalDiscount.Amount.Should().Be(0m);
        zero.TotalTax.Amount.Should().Be(0m);
        zero.TotalToPay.Amount.Should().Be(0m);
    }
}
