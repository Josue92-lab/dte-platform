namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class DocumentTotals : ValueObject
{
    public Money TotalNonTaxable { get; }
    public Money TotalExempt { get; }
    public Money TotalTaxable { get; }
    public Money SubTotalSales { get; }
    public Money TotalDiscount { get; }
    public Money TotalTax { get; }
    public Money TotalToPay { get; }

    public static readonly DocumentTotals Zero = new(
        Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero);

    private DocumentTotals(
        Money totalNonTaxable,
        Money totalExempt,
        Money totalTaxable,
        Money subTotalSales,
        Money totalDiscount,
        Money totalTax,
        Money totalToPay)
    {
        TotalNonTaxable = totalNonTaxable;
        TotalExempt = totalExempt;
        TotalTaxable = totalTaxable;
        SubTotalSales = subTotalSales;
        TotalDiscount = totalDiscount;
        TotalTax = totalTax;
        TotalToPay = totalToPay;
    }

    public static DocumentTotals Create(
        Money totalNonTaxable,
        Money totalExempt,
        Money totalTaxable,
        Money totalDiscount,
        Money totalTax)
    {
        var subTotalSales = totalNonTaxable + totalExempt + totalTaxable;
        var totalToPay = subTotalSales + totalTax - totalDiscount;

        return new DocumentTotals(
            totalNonTaxable,
            totalExempt,
            totalTaxable,
            subTotalSales,
            totalDiscount,
            totalTax,
            totalToPay);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return TotalNonTaxable;
        yield return TotalExempt;
        yield return TotalTaxable;
        yield return SubTotalSales;
        yield return TotalDiscount;
        yield return TotalTax;
        yield return TotalToPay;
    }
}
