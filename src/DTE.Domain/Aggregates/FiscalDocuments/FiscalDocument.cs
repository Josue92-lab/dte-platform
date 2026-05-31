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

    private readonly List<DocumentLine> _lines = new();
    public IReadOnlyCollection<DocumentLine> Lines => _lines.AsReadOnly();

    public DocumentTotals Totals { get; private set; } = DocumentTotals.Zero;

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
            recipient)
        {
            CreatedAtUtc = createdOnUtc
        };

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

    public Result<DocumentLine> AddLine(
        Quantity quantity,
        int unitOfMeasure,
        string description,
        Money unitPrice,
        Money discountAmount,
        Money nonTaxableAmount,
        Money exemptAmount,
        Money taxableAmount,
        Money taxAmount)
    {
        if (Status != FiscalDocumentStatus.Draft)
        {
            return Result.Failure<DocumentLine>(new Error("FiscalDocument.InvalidStateForModification", "Cannot modify document unless it is in Draft status."));
        }

        if (_lines.Count >= 2000)
        {
            return Result.Failure<DocumentLine>(new Error("FiscalDocument.LineLimitExceeded", "Document cannot exceed 2000 line items."));
        }

        if (quantity is null)
        {
            return Result.Failure<DocumentLine>(new Error("DocumentLine.QuantityNull", "Quantity is required."));
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length > 1500)
        {
            return Result.Failure<DocumentLine>(new Error("DocumentLine.DescriptionInvalidLength", "Description must be between 1 and 1500 characters."));
        }

        if (unitPrice is null)
        {
            return Result.Failure<DocumentLine>(new Error("DocumentLine.UnitPriceNull", "UnitPrice is required."));
        }

        if (discountAmount is null)
        {
            return Result.Failure<DocumentLine>(new Error("DocumentLine.DiscountAmountNull", "DiscountAmount is required."));
        }

        if (nonTaxableAmount is null)
        {
            return Result.Failure<DocumentLine>(new Error("DocumentLine.NonTaxableAmountNull", "NonTaxableAmount is required."));
        }

        if (exemptAmount is null)
        {
            return Result.Failure<DocumentLine>(new Error("DocumentLine.ExemptAmountNull", "ExemptAmount is required."));
        }

        if (taxableAmount is null)
        {
            return Result.Failure<DocumentLine>(new Error("DocumentLine.TaxableAmountNull", "TaxableAmount is required."));
        }

        if (taxAmount is null)
        {
            return Result.Failure<DocumentLine>(new Error("DocumentLine.TaxAmountNull", "TaxAmount is required."));
        }

        var numItem = _lines.Count + 1;
        var line = new DocumentLine(
            Guid.NewGuid(),
            numItem,
            quantity,
            unitOfMeasure,
            description,
            unitPrice,
            discountAmount,
            nonTaxableAmount,
            exemptAmount,
            taxableAmount,
            taxAmount);

        _lines.Add(line);
        RecalculateTotals();

        return Result.Success(line);
    }

    public Result RemoveLine(int numItem)
    {
        if (Status != FiscalDocumentStatus.Draft)
        {
            return Result.Failure(new Error("FiscalDocument.InvalidStateForModification", "Cannot modify document unless it is in Draft status."));
        }

        var line = _lines.SingleOrDefault(l => l.NumItem == numItem);
        if (line is null)
        {
            return Result.Failure(new Error("FiscalDocument.LineNotFound", "Line item not found."));
        }

        _lines.Remove(line);

        // Re-sequence the remaining lines
        for (int i = 0; i < _lines.Count; i++)
        {
            _lines[i].UpdateNumItem(i + 1);
        }

        RecalculateTotals();

        return Result.Success();
    }

    private void RecalculateTotals()
    {
        if (_lines.Count == 0)
        {
            Totals = DocumentTotals.Zero;
            return;
        }

        var nonTaxable = Money.Zero;
        var exempt = Money.Zero;
        var taxable = Money.Zero;
        var discount = Money.Zero;
        var tax = Money.Zero;

        foreach (var line in _lines)
        {
            nonTaxable += line.NonTaxableAmount;
            exempt += line.ExemptAmount;
            taxable += line.TaxableAmount;
            discount += line.DiscountAmount;
            tax += line.TaxAmount;
        }

        Totals = DocumentTotals.Create(nonTaxable, exempt, taxable, discount, tax);
    }
}
