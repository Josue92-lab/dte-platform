using DTE.Domain.Aggregates.FiscalDocuments;
using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.Aggregates.FiscalDocuments;

public class FiscalDocumentStateMachineTests
{
    private static IssuerSnapshot ValidIssuer() => IssuerSnapshot.Create(
        TaxIdentifier.Create("NIT", "06141804941035").Value,
        TaxIdentifier.Create("NRC", "1234567").Value,
        PartyName.Create("Empresa Ejemplo S.A. de C.V.", "Mi Tienda").Value,
        EconomicActivityCode.Create("46100", "Comercio al por mayor").Value,
        Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Colonia Escalón #123").Value,
        PhoneNumber.Create("22001234").Value,
        EmailAddress.Create("info@empresa.com").Value).Value;

    private static DocumentSignature ValidSignature() => DocumentSignature.Create("abc123signaturehash").Value;
    private static ReceptionStamp ValidStamp() => ReceptionStamp.Create("2026ABCDEF123456SELLO").Value;
    private static RejectionReason ValidRejection() => RejectionReason.Create("MH-001", "Invalid document structure.").Value;

    private static FiscalDocument CreateValidDraftWithLines()
    {
        var now = DateTime.UtcNow;
        var document = FiscalDocument.Create(
            DocumentId.New(), DteType.FacturaElectronica, 2, EnvironmentType.Test, OperationType.Normal,
            DateOnly.FromDateTime(now), TimeOnly.FromDateTime(now),
            ValidIssuer(), null, now).Value;

        document.AddLine(
            Quantity.Create(1m).Value, 59, "Valid item",
            Money.Create(100m).Value, Money.Create(0m).Value,
            Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(100m).Value, Money.Create(13m).Value);

        return document;
    }

    private static FiscalDocument CreateValidatedDocument()
    {
        var document = CreateValidDraftWithLines();
        document.MarkAsValidated();
        return document;
    }

    private static FiscalDocument CreateSignedDocument()
    {
        var document = CreateValidatedDocument();
        document.MarkAsSigned(ValidSignature());
        return document;
    }

    // ===================================================================
    // Happy Path Transitions
    // ===================================================================

    [Fact]
    public void FullLifecycle_Draft_Validated_Signed_Processed_ShouldSucceed()
    {
        var document = CreateValidDraftWithLines();
        document.Status.Should().Be(FiscalDocumentStatus.Draft);

        var validationResult = document.MarkAsValidated();
        validationResult.IsValid.Should().BeTrue();
        document.Status.Should().Be(FiscalDocumentStatus.Validated);

        var signResult = document.MarkAsSigned(ValidSignature());
        signResult.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(FiscalDocumentStatus.Signed);
        document.Signature.Should().NotBeNull();

        var processResult = document.MarkAsProcessed(ValidStamp());
        processResult.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(FiscalDocumentStatus.Processed);
        document.ReceptionStamp.Should().NotBeNull();
    }

    [Fact]
    public void FullLifecycle_Draft_Validated_Signed_Rejected_ShouldSucceed()
    {
        var document = CreateValidDraftWithLines();

        document.MarkAsValidated();
        document.MarkAsSigned(ValidSignature());

        var rejectResult = document.MarkAsRejected(ValidRejection());
        rejectResult.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(FiscalDocumentStatus.Rejected);
        document.RejectionReason.Should().NotBeNull();
        document.RejectionReason!.Code.Should().Be("MH-001");
    }

    // ===================================================================
    // Validation Gate Tests
    // ===================================================================

    [Fact]
    public void MarkAsValidated_ShouldFail_WhenDocumentIsInvalid()
    {
        var now = DateTime.UtcNow;
        var document = FiscalDocument.Create(
            DocumentId.New(), DteType.FacturaElectronica, 2, EnvironmentType.Test, OperationType.Normal,
            DateOnly.FromDateTime(now), TimeOnly.FromDateTime(now),
            ValidIssuer(), null, now).Value;

        // No lines added — validation will fail.
        var result = document.MarkAsValidated();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.DocumentMustHaveLines");
        document.Status.Should().Be(FiscalDocumentStatus.Draft);
    }

    [Fact]
    public void MarkAsValidated_ShouldSucceed_WhenDocumentIsValid()
    {
        var document = CreateValidDraftWithLines();

        var result = document.MarkAsValidated();

        result.IsValid.Should().BeTrue();
        document.Status.Should().Be(FiscalDocumentStatus.Validated);
    }

    [Fact]
    public void MarkAsValidated_ShouldFail_WhenAlreadyValidated()
    {
        var document = CreateValidatedDocument();

        var result = document.MarkAsValidated();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "FiscalDocument.InvalidStateForValidation");
        document.Status.Should().Be(FiscalDocumentStatus.Validated);
    }

    // ===================================================================
    // Invalid Transition Tests
    // ===================================================================

    [Fact]
    public void MarkAsSigned_ShouldFail_WhenStatusIsDraft()
    {
        var document = CreateValidDraftWithLines();

        var result = document.MarkAsSigned(ValidSignature());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForSigning");
        document.Status.Should().Be(FiscalDocumentStatus.Draft);
    }

    [Fact]
    public void MarkAsProcessed_ShouldFail_WhenStatusIsDraft()
    {
        var document = CreateValidDraftWithLines();

        var result = document.MarkAsProcessed(ValidStamp());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForProcessing");
    }

    [Fact]
    public void MarkAsProcessed_ShouldFail_WhenStatusIsValidated()
    {
        var document = CreateValidatedDocument();

        var result = document.MarkAsProcessed(ValidStamp());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForProcessing");
    }

    [Fact]
    public void MarkAsRejected_ShouldFail_WhenStatusIsDraft()
    {
        var document = CreateValidDraftWithLines();

        var result = document.MarkAsRejected(ValidRejection());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForRejection");
    }

    [Fact]
    public void MarkAsRejected_ShouldFail_WhenStatusIsValidated()
    {
        var document = CreateValidatedDocument();

        var result = document.MarkAsRejected(ValidRejection());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForRejection");
    }

    // ===================================================================
    // Null Guard Tests
    // ===================================================================

    [Fact]
    public void MarkAsSigned_ShouldFail_WhenSignatureIsNull()
    {
        var document = CreateValidatedDocument();

        var result = document.MarkAsSigned(null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.SignatureNull");
    }

    [Fact]
    public void MarkAsProcessed_ShouldFail_WhenStampIsNull()
    {
        var document = CreateSignedDocument();

        var result = document.MarkAsProcessed(null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.ReceptionStampNull");
    }

    [Fact]
    public void MarkAsRejected_ShouldFail_WhenReasonIsNull()
    {
        var document = CreateSignedDocument();

        var result = document.MarkAsRejected(null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.RejectionReasonNull");
    }

    // ===================================================================
    // Terminal State Tests — Processed
    // ===================================================================

    [Fact]
    public void Processed_ShouldRejectMarkAsSigned()
    {
        var document = CreateSignedDocument();
        document.MarkAsProcessed(ValidStamp());

        var result = document.MarkAsSigned(ValidSignature());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForSigning");
    }

    [Fact]
    public void Processed_ShouldRejectMarkAsProcessed()
    {
        var document = CreateSignedDocument();
        document.MarkAsProcessed(ValidStamp());

        var result = document.MarkAsProcessed(ValidStamp());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForProcessing");
    }

    [Fact]
    public void Processed_ShouldRejectMarkAsRejected()
    {
        var document = CreateSignedDocument();
        document.MarkAsProcessed(ValidStamp());

        var result = document.MarkAsRejected(ValidRejection());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForRejection");
    }

    [Fact]
    public void Processed_ShouldRejectAddLine()
    {
        var document = CreateSignedDocument();
        document.MarkAsProcessed(ValidStamp());

        var result = document.AddLine(
            Quantity.Create(1m).Value, 59, "New item",
            Money.Create(10m).Value, Money.Zero, Money.Zero, Money.Zero, Money.Create(10m).Value, Money.Create(1.3m).Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForModification");
    }

    [Fact]
    public void Processed_ShouldRejectRemoveLine()
    {
        var document = CreateSignedDocument();
        document.MarkAsProcessed(ValidStamp());

        var result = document.RemoveLine(1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForModification");
    }

    [Fact]
    public void Processed_ShouldRejectMarkAsValidated()
    {
        var document = CreateSignedDocument();
        document.MarkAsProcessed(ValidStamp());

        var result = document.MarkAsValidated();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "FiscalDocument.InvalidStateForValidation");
    }

    // ===================================================================
    // Terminal State Tests — Rejected
    // ===================================================================

    [Fact]
    public void Rejected_ShouldRejectMarkAsSigned()
    {
        var document = CreateSignedDocument();
        document.MarkAsRejected(ValidRejection());

        var result = document.MarkAsSigned(ValidSignature());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForSigning");
    }

    [Fact]
    public void Rejected_ShouldRejectMarkAsProcessed()
    {
        var document = CreateSignedDocument();
        document.MarkAsRejected(ValidRejection());

        var result = document.MarkAsProcessed(ValidStamp());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForProcessing");
    }

    [Fact]
    public void Rejected_ShouldRejectMarkAsRejected()
    {
        var document = CreateSignedDocument();
        document.MarkAsRejected(ValidRejection());

        var result = document.MarkAsRejected(ValidRejection());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForRejection");
    }

    [Fact]
    public void Rejected_ShouldRejectAddLine()
    {
        var document = CreateSignedDocument();
        document.MarkAsRejected(ValidRejection());

        var result = document.AddLine(
            Quantity.Create(1m).Value, 59, "New item",
            Money.Create(10m).Value, Money.Zero, Money.Zero, Money.Zero, Money.Create(10m).Value, Money.Create(1.3m).Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForModification");
    }

    [Fact]
    public void Rejected_ShouldRejectRemoveLine()
    {
        var document = CreateSignedDocument();
        document.MarkAsRejected(ValidRejection());

        var result = document.RemoveLine(1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForModification");
    }

    [Fact]
    public void Rejected_ShouldRejectMarkAsValidated()
    {
        var document = CreateSignedDocument();
        document.MarkAsRejected(ValidRejection());

        var result = document.MarkAsValidated();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "FiscalDocument.InvalidStateForValidation");
    }

    // ===================================================================
    // Immutability Tests (Non-Draft States)
    // ===================================================================

    [Fact]
    public void AddLine_ShouldFail_WhenStatusIsValidated()
    {
        var document = CreateValidatedDocument();

        var result = document.AddLine(
            Quantity.Create(1m).Value, 59, "New item",
            Money.Create(10m).Value, Money.Zero, Money.Zero, Money.Zero, Money.Create(10m).Value, Money.Create(1.3m).Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForModification");
    }

    [Fact]
    public void RemoveLine_ShouldFail_WhenStatusIsValidated()
    {
        var document = CreateValidatedDocument();

        var result = document.RemoveLine(1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForModification");
    }

    [Fact]
    public void AddLine_ShouldFail_WhenStatusIsSigned()
    {
        var document = CreateSignedDocument();

        var result = document.AddLine(
            Quantity.Create(1m).Value, 59, "New item",
            Money.Create(10m).Value, Money.Zero, Money.Zero, Money.Zero, Money.Create(10m).Value, Money.Create(1.3m).Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForModification");
    }

    [Fact]
    public void RemoveLine_ShouldFail_WhenStatusIsSigned()
    {
        var document = CreateSignedDocument();

        var result = document.RemoveLine(1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidStateForModification");
    }
}
