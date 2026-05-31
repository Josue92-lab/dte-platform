using DTE.Domain.Aggregates.FiscalDocuments;
using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.Aggregates.FiscalDocuments;

public class FiscalDocumentValidationTests
{
    private static IssuerSnapshot ValidIssuer() => IssuerSnapshot.Create(
        TaxIdentifier.Create("NIT", "06141804941035").Value,
        TaxIdentifier.Create("NRC", "1234567").Value,
        PartyName.Create("Empresa Ejemplo S.A. de C.V.", "Mi Tienda").Value,
        EconomicActivityCode.Create("46100", "Comercio al por mayor").Value,
        Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Colonia Escalón #123").Value,
        PhoneNumber.Create("22001234").Value,
        EmailAddress.Create("info@empresa.com").Value).Value;

    private static RecipientSnapshot MinimalRecipient() => RecipientSnapshot.Create(
        PartyName.Create("Consumidor Final").Value).Value;

    private static RecipientSnapshot FullRecipient() => RecipientSnapshot.Create(
        PartyName.Create("Empresa Receptora S.A.").Value,
        TaxIdentifier.Create("NIT", "06141804941036").Value,
        TaxIdentifier.Create("NRC", "7654321").Value,
        EconomicActivityCode.Create("46200", "Servicios profesionales").Value,
        Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Calle Principal #456").Value,
        PhoneNumber.Create("22005678").Value,
        EmailAddress.Create("contacto@receptora.com").Value).Value;

    private static RecipientSnapshot RecipientWithoutIdentifier() => RecipientSnapshot.Create(
        PartyName.Create("Empresa Sin NIT").Value,
        documentIdentifier: null,
        nrc: TaxIdentifier.Create("NRC", "7654321").Value,
        economicActivity: EconomicActivityCode.Create("46200", "Servicios").Value,
        address: Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Calle #1").Value).Value;

    private static RecipientSnapshot RecipientWithoutAddress() => RecipientSnapshot.Create(
        PartyName.Create("Empresa Sin Dirección").Value,
        documentIdentifier: TaxIdentifier.Create("NIT", "06141804941036").Value,
        nrc: TaxIdentifier.Create("NRC", "7654321").Value,
        economicActivity: EconomicActivityCode.Create("46200", "Servicios").Value,
        address: null).Value;

    private static RecipientSnapshot RecipientWithoutEconomicActivity() => RecipientSnapshot.Create(
        PartyName.Create("Empresa Sin Actividad").Value,
        documentIdentifier: TaxIdentifier.Create("NIT", "06141804941036").Value,
        nrc: TaxIdentifier.Create("NRC", "7654321").Value,
        economicActivity: null,
        address: Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Calle #1").Value).Value;

    private static RecipientSnapshot RecipientWithNullNrc() => RecipientSnapshot.Create(
        PartyName.Create("Persona Natural").Value,
        documentIdentifier: TaxIdentifier.Create("NIT", "06141804941036").Value,
        nrc: null,
        economicActivity: EconomicActivityCode.Create("46200", "Servicios").Value,
        address: Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Calle #1").Value).Value;

    private static RecipientSnapshot RecipientWithoutIdentifierAndAddress() => RecipientSnapshot.Create(
        PartyName.Create("Empresa Incompleta").Value,
        documentIdentifier: null,
        nrc: null,
        economicActivity: EconomicActivityCode.Create("46200", "Servicios").Value,
        address: null).Value;

    private static FiscalDocument CreateValidFeDocument()
    {
        var now = DateTime.UtcNow;
        return FiscalDocument.Create(
            DocumentId.New(), DteType.FacturaElectronica, 2, EnvironmentType.Test, OperationType.Normal,
            DateOnly.FromDateTime(now), TimeOnly.FromDateTime(now),
            ValidIssuer(), null, now).Value;
    }

    private static FiscalDocument CreateValidCcfDocument(RecipientSnapshot? recipient = null)
    {
        var now = DateTime.UtcNow;
        return FiscalDocument.Create(
            DocumentId.New(), DteType.ComprobanteCreditoFiscal, 4, EnvironmentType.Test, OperationType.Normal,
            DateOnly.FromDateTime(now), TimeOnly.FromDateTime(now),
            ValidIssuer(), recipient, now).Value;
    }

    private static void AddValidLine(FiscalDocument document)
    {
        document.AddLine(
            Quantity.Create(1m).Value, 59, "Valid item",
            Money.Create(100m).Value, Money.Create(0m).Value,
            Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(100m).Value, Money.Create(13m).Value);
    }

    // ===================================================================
    // ValidationResult Notification Behavior
    // ===================================================================

    [Fact]
    public void Validate_ShouldReturnAllViolations_NotJustFirst()
    {
        // Document with zero lines AND negative totals should produce multiple errors.
        var document = CreateValidFeDocument();
        // No lines added → U-01 will fire.
        // Totals are zero (no lines), so U-03/U-04 won't fire (0 is not < 0).

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.DocumentMustHaveLines");
    }

    // ===================================================================
    // Universal Rules (All DTE Types)
    // ===================================================================

    [Fact]
    public void Validate_ShouldFail_WhenDocumentHasNoLines()
    {
        var document = CreateValidFeDocument();

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.DocumentMustHaveLines");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTotalToPayIsNegative()
    {
        var document = CreateValidFeDocument();

        // Add line where discount exceeds sub-total: SubTotal=10, Tax=0, Discount=25 → TotalToPay = 10 + 0 - 25 = -15
        document.AddLine(
            Quantity.Create(1m).Value, 59, "Discounted item",
            Money.Create(10m).Value, Money.Create(25m).Value,
            Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(10m).Value, Money.Create(0m).Value);

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.TotalToPayMustNotBeNegative");
    }

    [Fact]
    public void Validate_ShouldFail_WhenLineHasNoEconomicValue()
    {
        var document = CreateValidFeDocument();

        // Add a valid line first, then a zero-value line.
        AddValidLine(document);
        document.AddLine(
            Quantity.Create(1m).Value, 59, "Zero-value item",
            Money.Create(0m).Value, Money.Create(0m).Value,
            Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(0m).Value);

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.LineHasNoEconomicValue");
    }

    [Fact]
    public void Validate_ShouldReportMultipleViolationsSimultaneously()
    {
        var document = CreateValidFeDocument();

        // Add a zero-value line AND a negative-totals scenario.
        // Zero-value line: all categories zero.
        document.AddLine(
            Quantity.Create(1m).Value, 59, "Zero-value item",
            Money.Create(0m).Value, Money.Create(0m).Value,
            Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(0m).Value);

        // Now add a line with discount exceeding sub-total.
        document.AddLine(
            Quantity.Create(1m).Value, 59, "High-discount item",
            Money.Create(5m).Value, Money.Create(50m).Value,
            Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(5m).Value, Money.Create(0m).Value);

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.LineHasNoEconomicValue");
        result.Errors.Should().Contain(e => e.Code == "Validation.TotalToPayMustNotBeNegative");
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    // ===================================================================
    // Factura Electrónica (FE) Rules
    // ===================================================================

    [Fact]
    public void Validate_FE_ShouldReturnValid_WhenDocumentIsComplete()
    {
        var document = CreateValidFeDocument();
        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_FE_ShouldReturnValid_WithNoRecipient()
    {
        // FE permits null recipient.
        var document = CreateValidFeDocument();
        AddValidLine(document);

        document.Recipient.Should().BeNull();

        var result = document.Validate();

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FE_ShouldReturnValid_WithRecipient()
    {
        var now = DateTime.UtcNow;
        var document = FiscalDocument.Create(
            DocumentId.New(), DteType.FacturaElectronica, 2, EnvironmentType.Test, OperationType.Normal,
            DateOnly.FromDateTime(now), TimeOnly.FromDateTime(now),
            ValidIssuer(), MinimalRecipient(), now).Value;

        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeTrue();
    }

    // ===================================================================
    // Comprobante de Crédito Fiscal (CCF) Rules
    // ===================================================================

    [Fact]
    public void Validate_CCF_ShouldFail_WhenRecipientIsNull()
    {
        var document = CreateValidCcfDocument(null);
        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.CCF.RecipientRequired");
    }

    [Fact]
    public void Validate_CCF_ShouldFail_WhenRecipientLacksDocumentIdentifier()
    {
        var document = CreateValidCcfDocument(RecipientWithoutIdentifier());
        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.CCF.RecipientIdentifierRequired");
    }

    [Fact]
    public void Validate_CCF_ShouldFail_WhenRecipientLacksAddress()
    {
        var document = CreateValidCcfDocument(RecipientWithoutAddress());
        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.CCF.RecipientAddressRequired");
    }

    [Fact]
    public void Validate_CCF_ShouldFail_WhenRecipientLacksEconomicActivity()
    {
        var document = CreateValidCcfDocument(RecipientWithoutEconomicActivity());
        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.CCF.RecipientEconomicActivityRequired");
    }

    [Fact]
    public void Validate_CCF_ShouldReturnValid_WhenRecipientHasNullNrc()
    {
        // NRC is nullable per fe-ccf-v4.json — no error should be produced.
        var document = CreateValidCcfDocument(RecipientWithNullNrc());
        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.Code.Contains("Nrc"));
    }

    [Fact]
    public void Validate_CCF_ShouldReturnValid_WhenRecipientIsFullyIdentified()
    {
        var document = CreateValidCcfDocument(FullRecipient());
        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_CCF_ShouldReportMultipleCcfViolations()
    {
        // Recipient missing both DocumentIdentifier and Address.
        var document = CreateValidCcfDocument(RecipientWithoutIdentifierAndAddress());
        AddValidLine(document);

        var result = document.Validate();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Validation.CCF.RecipientIdentifierRequired");
        result.Errors.Should().Contain(e => e.Code == "Validation.CCF.RecipientAddressRequired");
    }

    // ===================================================================
    // Integration Tests (Full Document Lifecycle)
    // ===================================================================

    [Fact]
    public void Validate_ShouldReturnValid_ForCompleteFEDocument()
    {
        var now = DateTime.UtcNow;
        var document = FiscalDocument.Create(
            DocumentId.New(), DteType.FacturaElectronica, 2, EnvironmentType.Test, OperationType.Normal,
            DateOnly.FromDateTime(now), TimeOnly.FromDateTime(now),
            ValidIssuer(), MinimalRecipient(), now).Value;

        document.AddLine(
            Quantity.Create(2m).Value, 59, "Producto A",
            Money.Create(50m).Value, Money.Create(0m).Value,
            Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(100m).Value, Money.Create(13m).Value);

        document.AddLine(
            Quantity.Create(1m).Value, 59, "Servicio B",
            Money.Create(200m).Value, Money.Create(10m).Value,
            Money.Create(0m).Value, Money.Create(200m).Value, Money.Create(0m).Value, Money.Create(0m).Value);

        var result = document.Validate();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnValid_ForCompleteCCFDocument()
    {
        var document = CreateValidCcfDocument(FullRecipient());

        document.AddLine(
            Quantity.Create(5m).Value, 59, "Insumos industriales",
            Money.Create(1000m).Value, Money.Create(0m).Value,
            Money.Create(0m).Value, Money.Create(0m).Value, Money.Create(5000m).Value, Money.Create(650m).Value);

        var result = document.Validate();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
