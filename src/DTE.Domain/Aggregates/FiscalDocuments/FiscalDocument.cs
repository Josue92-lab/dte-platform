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

    private FiscalDocument(
        Guid id,
        DocumentId documentId,
        DteType dteType) 
        : base(id)
    {
        DocumentId = documentId;
        DteType = dteType;
        Status = FiscalDocumentStatus.Draft;
    }

    public static Result<FiscalDocument> Create(DocumentId documentId, DteType dteType, DateTime createdOnUtc)
    {
        if (documentId is null)
        {
            return Result.Failure<FiscalDocument>(new Error("FiscalDocument.DocumentIdNull", "DocumentId is required."));
        }

        if (dteType is null)
        {
            return Result.Failure<FiscalDocument>(new Error("FiscalDocument.DteTypeNull", "DteType is required."));
        }

        var document = new FiscalDocument(
            documentId.Value,
            documentId,
            dteType);

        document.CreatedAtUtc = createdOnUtc;

        document.RaiseDomainEvent(new FiscalDocumentCreated(
            Guid.NewGuid(),
            createdOnUtc,
            documentId,
            dteType));

        return Result.Success(document);
    }
}
