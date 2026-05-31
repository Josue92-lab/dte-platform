namespace DTE.Domain.Aggregates.FiscalDocuments;

using DTE.Domain.Primitives;
using DTE.Domain.ValueObjects;

public sealed class DocumentLine : Entity
{
    public int NumItem { get; private set; }
    public Quantity Quantity { get; private set; }
    public int UnitOfMeasure { get; private set; }
    public string Description { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money NonTaxableAmount { get; private set; }
    public Money ExemptAmount { get; private set; }
    public Money TaxableAmount { get; private set; }
    public Money TaxAmount { get; private set; }

    internal DocumentLine(
        Guid id,
        int numItem,
        Quantity quantity,
        int unitOfMeasure,
        string description,
        Money unitPrice,
        Money discountAmount,
        Money nonTaxableAmount,
        Money exemptAmount,
        Money taxableAmount,
        Money taxAmount) : base(id)
    {
        NumItem = numItem;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
        Description = description;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        NonTaxableAmount = nonTaxableAmount;
        ExemptAmount = exemptAmount;
        TaxableAmount = taxableAmount;
        TaxAmount = taxAmount;
    }

    internal void UpdateNumItem(int newNumItem)
    {
        NumItem = newNumItem;
    }
}
