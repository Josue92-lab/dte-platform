namespace DTE.Domain.Events;

using DTE.Domain.Primitives;
using DTE.Domain.ValueObjects;

public sealed record ControlNumberSeriesCreated(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid SeriesId,
    DteType DteType) : DomainEvent(Id, OccurredOnUtc);
