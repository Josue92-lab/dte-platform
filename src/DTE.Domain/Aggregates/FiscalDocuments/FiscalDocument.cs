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

    public ValidationResult Validate()
    {
        var result = new ValidationResult();

        ValidateUniversalRules(result);

        if (DteType == DteType.FacturaElectronica)
        {
            ValidateFacturaElectronicaRules(result);
        }
        else if (DteType == DteType.ComprobanteCreditoFiscal)
        {
            ValidateCreditoFiscalRules(result);
        }

        return result;
    }

    private void ValidateUniversalRules(ValidationResult result)
    {
        // U-01: Document must have at least one line.
        if (_lines.Count == 0)
        {
            result.AddError(new ValidationError(
                "Validation.DocumentMustHaveLines",
                "A fiscal document must contain at least one line item.",
                "Lines"));
        }

        // U-02: Issuer is required.
        if (Issuer is null)
        {
            result.AddError(new ValidationError(
                "Validation.IssuerRequired",
                "Issuer is required.",
                "Issuer"));
        }

        // U-03: TotalToPay must not be negative.
        if (Totals.TotalToPay.Amount < 0)
        {
            result.AddError(new ValidationError(
                "Validation.TotalToPayMustNotBeNegative",
                "Total to pay must not be negative.",
                "Totals.TotalToPay"));
        }

        // U-04: SubTotalSales must not be negative.
        if (Totals.SubTotalSales.Amount < 0)
        {
            result.AddError(new ValidationError(
                "Validation.SubTotalSalesMustNotBeNegative",
                "Sub-total sales must not be negative.",
                "Totals.SubTotalSales"));
        }

        // U-05: IssueDate is required (must not be default).
        if (IssueDate == DateOnly.MinValue)
        {
            result.AddError(new ValidationError(
                "Validation.IssueDateRequired",
                "Issue date is required.",
                "IssueDate"));
        }

        // U-06: DocumentVersion must be greater than zero.
        if (DocumentVersion <= 0)
        {
            result.AddError(new ValidationError(
                "Validation.DocumentVersionRequired",
                "Document version must be greater than zero.",
                "DocumentVersion"));
        }

        // U-07: No line should have zero economic value.
        for (int i = 0; i < _lines.Count; i++)
        {
            var line = _lines[i];
            if (line.NonTaxableAmount.Amount == 0m
                && line.ExemptAmount.Amount == 0m
                && line.TaxableAmount.Amount == 0m)
            {
                result.AddError(new ValidationError(
                    "Validation.LineHasNoEconomicValue",
                    $"Line item {line.NumItem} has no economic value (all sale categories are zero).",
                    $"Lines[{line.NumItem}]"));
            }
        }

        // U-08: Lines must be sequentially numbered 1..N.
        for (int i = 0; i < _lines.Count; i++)
        {
            if (_lines[i].NumItem != i + 1)
            {
                result.AddError(new ValidationError(
                    "Validation.NumItemSequenceInvalid",
                    "Line items are not sequentially numbered from 1.",
                    "Lines"));
                break;
            }
        }
    }

    private static void ValidateFacturaElectronicaRules(ValidationResult result)
    {
        // FE-01: Guard clause only.
        // FE permits a null Recipient — no additional constraints.
        _ = result; // Explicit no-op; reserved for future FE-specific rules.
    }

    private void ValidateCreditoFiscalRules(ValidationResult result)
    {
        // CCF-01: Recipient is required.
        if (Recipient is null)
        {
            result.AddError(new ValidationError(
                "Validation.CCF.RecipientRequired",
                "A CCF document requires a recipient.",
                "Recipient"));
            return; // Cannot inspect Recipient properties if null.
        }

        // CCF-02: Recipient must have a document identifier (NIT).
        if (Recipient.DocumentIdentifier is null)
        {
            result.AddError(new ValidationError(
                "Validation.CCF.RecipientIdentifierRequired",
                "CCF recipient must have a document identifier (NIT).",
                "Recipient.DocumentIdentifier"));
        }

        // CCF-03: Recipient must have an address.
        if (Recipient.Address is null)
        {
            result.AddError(new ValidationError(
                "Validation.CCF.RecipientAddressRequired",
                "CCF recipient must have an address.",
                "Recipient.Address"));
        }

        // CCF-04: Recipient must have an economic activity.
        if (Recipient.EconomicActivity is null)
        {
            result.AddError(new ValidationError(
                "Validation.CCF.RecipientEconomicActivityRequired",
                "CCF recipient must have an economic activity.",
                "Recipient.EconomicActivity"));
        }

        // NRC is intentionally NOT validated — nullable per fe-ccf-v4.json.
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
