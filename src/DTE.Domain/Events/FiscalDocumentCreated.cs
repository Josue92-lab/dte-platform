namespace DTE.Domain.Events;

using DTE.Domain.Primitives;
using DTE.Domain.ValueObjects;
using DTE.Domain.Aggregates.FiscalDocuments;

public sealed record FiscalDocumentCreated(
    Guid Id,
    DateTime OccurredOnUtc,
    DocumentId DocumentId,
    DteType DteType,
    int DocumentVersion,
    EnvironmentType EnvironmentType,
    OperationType OperationType,
    DateOnly IssueDate,
    TimeOnly IssueTime,
    IssuerSnapshot Issuer,
    RecipientSnapshot? Recipient) : DomainEvent(Id, OccurredOnUtc);
