namespace DTE.Domain.Aggregates.FiscalDocuments;

using DTE.Domain.Primitives;
using DTE.Domain.ValueObjects;
using DTE.Domain.Events;

public sealed class FiscalDocument : AggregateRoot
{
    public DocumentId DocumentId { get; private set; }
    public DteType DteType { get; private set; }
    public FiscalDocumentStatus Status { get; private set; }
    public ControlNumber? ControlNumber { get; private set; }
    public GenerationCode? GenerationCode { get; private set; }

    public int DocumentVersion { get; private set; }
    public EnvironmentType EnvironmentType { get; private set; }
    public OperationType OperationType { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public TimeOnly IssueTime { get; private set; }

    public IssuerSnapshot Issuer { get; private set; }
    public RecipientSnapshot? Recipient { get; private set; }

    private FiscalDocument(
        Guid id,
        DocumentId documentId,
        DteType dteType,
        int documentVersion,
        EnvironmentType environmentType,
        OperationType operationType,
        DateOnly issueDate,
        TimeOnly issueTime,
        IssuerSnapshot issuer,
        RecipientSnapshot? recipient)
        : base(id)
    {
        DocumentId = documentId;
        DteType = dteType;
        Status = FiscalDocumentStatus.Draft;
        DocumentVersion = documentVersion;
        EnvironmentType = environmentType;
        OperationType = operationType;
        IssueDate = issueDate;
        IssueTime = issueTime;
        Issuer = issuer;
        Recipient = recipient;
    }

    public static Result<FiscalDocument> Create(
        DocumentId documentId,
        DteType dteType,
        int documentVersion,
        EnvironmentType environmentType,
        OperationType operationType,
        DateOnly issueDate,
        TimeOnly issueTime,
        IssuerSnapshot issuer,
        RecipientSnapshot? recipient,
        DateTime createdOnUtc)
    {
        if (documentId is null)
        {
            return Result.Failure<FiscalDocument>(new Error("FiscalDocument.DocumentIdNull", "DocumentId is required."));
        }

        if (dteType is null)
        {
            return Result.Failure<FiscalDocument>(new Error("FiscalDocument.DteTypeNull", "DteType is required."));
        }

        // According to MH Schema, DocumentVersion is required and typically 1, 2, 3, etc.
        if (documentVersion <= 0)
        {
            return Result.Failure<FiscalDocument>(new Error("FiscalDocument.InvalidDocumentVersion", "DocumentVersion must be greater than zero."));
        }

        if (issuer is null)
        {
            return Result.Failure<FiscalDocument>(new Error("FiscalDocument.IssuerNull", "Issuer is required."));
        }

        var document = new FiscalDocument(
            documentId.Value,
            documentId,
            dteType,
            documentVersion,
            environmentType,
            operationType,
            issueDate,
            issueTime,
            issuer,
            recipient);

        document.CreatedAtUtc = createdOnUtc;

        document.RaiseDomainEvent(new FiscalDocumentCreated(
            Guid.NewGuid(),
            createdOnUtc,
            documentId,
            dteType,
            documentVersion,
            environmentType,
            operationType,
            issueDate,
            issueTime,
            issuer,
            recipient));

        return Result.Success(document);
    }
}
