namespace DTE.Domain.Events;

using DTE.Domain.Primitives;
using DTE.Domain.ValueObjects;

public sealed record ControlNumberAllocated(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid SeriesId,
    ControlNumber ControlNumber) : DomainEvent(Id, OccurredOnUtc);
