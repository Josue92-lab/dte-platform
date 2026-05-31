namespace DTE.Domain.ValueObjects;

using DTE.Domain.Primitives;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
        {
            return Result.Failure<Money>(new Error("Money.NegativeAmount", "Money amount cannot be negative."));
        }

        if (currency != "USD")
        {
            return Result.Failure<Money>(new Error("Money.InvalidCurrency", "Only USD is supported in V1."));
        }

        return Result.Success(new Money(amount, currency));
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
}
