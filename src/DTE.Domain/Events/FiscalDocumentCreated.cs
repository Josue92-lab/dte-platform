namespace DTE.Domain.Events;

using DTE.Domain.Primitives;
using DTE.Domain.ValueObjects;

public sealed record FiscalDocumentCreated(
    Guid Id,
    DateTime OccurredOnUtc,
    DocumentId DocumentId,
    DteType DteType) : DomainEvent(Id, OccurredOnUtc);
