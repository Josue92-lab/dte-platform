namespace DTE.Domain.Primitives;

public abstract record DomainEvent(Guid Id, DateTime OccurredOnUtc);
