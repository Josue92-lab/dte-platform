using DTE.Domain.Aggregates.FiscalDocuments;
using DTE.Domain.Events;
using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.Aggregates.FiscalDocuments;

public class FiscalDocumentTests
{
    private static IssuerSnapshot ValidIssuer() => IssuerSnapshot.Create(
        TaxIdentifier.Create("NIT", "06141804941035").Value,
        TaxIdentifier.Create("NRC", "1234567").Value,
        PartyName.Create("Empresa Ejemplo S.A. de C.V.", "Mi Tienda").Value,
        EconomicActivityCode.Create("46100", "Comercio al por mayor").Value,
        Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Colonia Escalón #123").Value,
        PhoneNumber.Create("22001234").Value,
        EmailAddress.Create("info@empresa.com").Value).Value;

    private static RecipientSnapshot ValidRecipient() => RecipientSnapshot.Create(
        PartyName.Create("Consumidor Final").Value).Value;

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidWithIssuerOnly()
    {
        var documentId = DocumentId.New();
        var dteType = DteType.FacturaElectronica;
        var now = DateTime.UtcNow;
        var issueDate = DateOnly.FromDateTime(now);
        var issueTime = TimeOnly.FromDateTime(now);
        var issuer = ValidIssuer();

        var result = FiscalDocument.Create(documentId, dteType, 2, EnvironmentType.Test, OperationType.Normal, issueDate, issueTime, issuer, null, now);

        result.IsSuccess.Should().BeTrue();

        var document = result.Value;
        document.DocumentId.Should().Be(documentId);
        document.DteType.Should().Be(dteType);
        document.Status.Should().Be(FiscalDocumentStatus.Draft);
        document.ControlNumber.Should().BeNull();
        document.GenerationCode.Should().BeNull();
        document.CreatedAtUtc.Should().Be(now);
        document.DocumentVersion.Should().Be(2);
        document.EnvironmentType.Should().Be(EnvironmentType.Test);
        document.OperationType.Should().Be(OperationType.Normal);
        document.IssueDate.Should().Be(issueDate);
        document.IssueTime.Should().Be(issueTime);
        document.Issuer.Should().Be(issuer);
        document.Recipient.Should().BeNull();

        var domainEvent = document.GetDomainEvents().SingleOrDefault() as FiscalDocumentCreated;
        domainEvent.Should().NotBeNull();
        domainEvent!.DocumentId.Should().Be(documentId);
        domainEvent.DteType.Should().Be(dteType);
        domainEvent.DocumentVersion.Should().Be(2);
        domainEvent.EnvironmentType.Should().Be(EnvironmentType.Test);
        domainEvent.OperationType.Should().Be(OperationType.Normal);
        domainEvent.Issuer.Should().Be(issuer);
        domainEvent.Recipient.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidWithBothParticipants()
    {
        var documentId = DocumentId.New();
        var dteType = DteType.FacturaElectronica;
        var now = DateTime.UtcNow;
        var issueDate = DateOnly.FromDateTime(now);
        var issueTime = TimeOnly.FromDateTime(now);
        var issuer = ValidIssuer();
        var recipient = ValidRecipient();

        var result = FiscalDocument.Create(documentId, dteType, 2, EnvironmentType.Test, OperationType.Normal, issueDate, issueTime, issuer, recipient, now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Issuer.Should().Be(issuer);
        result.Value.Recipient.Should().Be(recipient);

        var domainEvent = result.Value.GetDomainEvents().SingleOrDefault() as FiscalDocumentCreated;
        domainEvent!.Issuer.Should().Be(issuer);
        domainEvent.Recipient.Should().Be(recipient);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDocumentIdIsNull()
    {
        var dteType = DteType.FacturaElectronica;
        var result = FiscalDocument.Create(null!, dteType, 2, EnvironmentType.Test, OperationType.Normal, DateOnly.MinValue, TimeOnly.MinValue, ValidIssuer(), null, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.DocumentIdNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDteTypeIsNull()
    {
        var documentId = DocumentId.New();
        var result = FiscalDocument.Create(documentId, null!, 2, EnvironmentType.Test, OperationType.Normal, DateOnly.MinValue, TimeOnly.MinValue, ValidIssuer(), null, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.DteTypeNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDocumentVersionIsInvalid()
    {
        var documentId = DocumentId.New();
        var result = FiscalDocument.Create(documentId, DteType.FacturaElectronica, 0, EnvironmentType.Test, OperationType.Normal, DateOnly.MinValue, TimeOnly.MinValue, ValidIssuer(), null, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidDocumentVersion");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenIssuerIsNull()
    {
        var documentId = DocumentId.New();
        var result = FiscalDocument.Create(documentId, DteType.FacturaElectronica, 2, EnvironmentType.Test, OperationType.Normal, DateOnly.MinValue, TimeOnly.MinValue, null!, null, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.IssuerNull");
    }

    [Fact]
    public void AddLine_ShouldRecalculateTotalsCorrectly()
    {
        var document = CreateValidDocument();

        document.AddLine(
            Quantity.Create(1m).Value, 59, "Item 1",
            Money.Create(10m).Value, Money.Create(0m).Value,
            Money.Create(5m).Value, Money.Create(0m).Value, Money.Create(10m).Value, Money.Create(1.3m).Value);

        document.AddLine(
            Quantity.Create(2m).Value, 59, "Item 2",
            Money.Create(20m).Value, Money.Create(5m).Value,
            Money.Create(0m).Value, Money.Create(10m).Value, Money.Create(35m).Value, Money.Create(4.55m).Value);

        document.Lines.Should().HaveCount(2);

        document.Totals.TotalNonTaxable.Amount.Should().Be(5m);
        document.Totals.TotalExempt.Amount.Should().Be(10m);
        document.Totals.TotalTaxable.Amount.Should().Be(45m);
        document.Totals.SubTotalSales.Amount.Should().Be(60m);
        document.Totals.TotalDiscount.Amount.Should().Be(5m);
        document.Totals.TotalTax.Amount.Should().Be(5.85m);
        document.Totals.TotalToPay.Amount.Should().Be(60.85m); // 60 + 5.85 - 5
    }

    [Fact]
    public void RemoveLine_ShouldResequenceNumItemAndRecalculateTotals()
    {
        var document = CreateValidDocument();

        document.AddLine(
            Quantity.Create(1m).Value, 59, "Item 1", Money.Zero, Money.Zero, Money.Create(10m).Value, Money.Zero, Money.Zero, Money.Zero);

        document.AddLine(
            Quantity.Create(1m).Value, 59, "Item 2", Money.Zero, Money.Zero, Money.Create(20m).Value, Money.Zero, Money.Zero, Money.Zero);

        document.AddLine(
            Quantity.Create(1m).Value, 59, "Item 3", Money.Zero, Money.Zero, Money.Create(30m).Value, Money.Zero, Money.Zero, Money.Zero);

        document.RemoveLine(2);

        document.Lines.Should().HaveCount(2);

        var lines = document.Lines.ToList();
        lines[0].NumItem.Should().Be(1);
        lines[0].Description.Should().Be("Item 1");

        lines[1].NumItem.Should().Be(2);
        lines[1].Description.Should().Be("Item 3");

        document.Totals.TotalNonTaxable.Amount.Should().Be(40m); // 10 + 30
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenDescriptionIsInvalid()
    {
        var document = CreateValidDocument();

        var result = document.AddLine(
            Quantity.Create(1m).Value, 59, "", Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentLine.DescriptionInvalidLength");
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenLimitExceeded()
    {
        var document = CreateValidDocument();

        for (int i = 0; i < 2000; i++)
        {
            document.AddLine(Quantity.Create(1m).Value, 59, "Item", Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero).IsSuccess.Should().BeTrue();
        }

        var result = document.AddLine(Quantity.Create(1m).Value, 59, "Item 2001", Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.LineLimitExceeded");
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenQuantityIsNull()
    {
        var document = CreateValidDocument();

        var result = document.AddLine(null!, 59, "Item", Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentLine.QuantityNull");
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenUnitPriceIsNull()
    {
        var document = CreateValidDocument();

        var result = document.AddLine(Quantity.Create(1m).Value, 59, "Item", null!, Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentLine.UnitPriceNull");
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenDiscountAmountIsNull()
    {
        var document = CreateValidDocument();

        var result = document.AddLine(Quantity.Create(1m).Value, 59, "Item", Money.Zero, null!, Money.Zero, Money.Zero, Money.Zero, Money.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentLine.DiscountAmountNull");
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenNonTaxableAmountIsNull()
    {
        var document = CreateValidDocument();

        var result = document.AddLine(Quantity.Create(1m).Value, 59, "Item", Money.Zero, Money.Zero, null!, Money.Zero, Money.Zero, Money.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentLine.NonTaxableAmountNull");
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenExemptAmountIsNull()
    {
        var document = CreateValidDocument();

        var result = document.AddLine(Quantity.Create(1m).Value, 59, "Item", Money.Zero, Money.Zero, Money.Zero, null!, Money.Zero, Money.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentLine.ExemptAmountNull");
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenTaxableAmountIsNull()
    {
        var document = CreateValidDocument();

        var result = document.AddLine(Quantity.Create(1m).Value, 59, "Item", Money.Zero, Money.Zero, Money.Zero, Money.Zero, null!, Money.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentLine.TaxableAmountNull");
    }

    [Fact]
    public void AddLine_ShouldReturnFailure_WhenTaxAmountIsNull()
    {
        var document = CreateValidDocument();

        var result = document.AddLine(Quantity.Create(1m).Value, 59, "Item", Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero, null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentLine.TaxAmountNull");
    }

    [Fact]
    public void RemoveLine_ShouldReturnFailure_WhenNumItemDoesNotExist()
    {
        var document = CreateValidDocument();
        document.AddLine(Quantity.Create(1m).Value, 59, "Item 1", Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero, Money.Zero);

        var result = document.RemoveLine(999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.LineNotFound");
    }

    [Fact]
    public void Totals_ShouldPreserveNegativeTotalToPay_WhenDiscountExceedsSubTotal()
    {
        var document = CreateValidDocument();

        // Line with discount larger than taxable amount: SubTotal=10, Tax=0, Discount=25 → TotalToPay = 10 + 0 - 25 = -15
        document.AddLine(
            Quantity.Create(1m).Value, 59, "Discounted item",
            Money.Create(10m).Value, Money.Create(25m).Value,
            Money.Zero, Money.Zero, Money.Create(10m).Value, Money.Zero);

        document.Totals.SubTotalSales.Amount.Should().Be(10m);
        document.Totals.TotalDiscount.Amount.Should().Be(25m);
        document.Totals.TotalToPay.Amount.Should().Be(-15m);
    }

    [Fact]
    public void Totals_ShouldResetToZero_WhenAllLinesRemoved()
    {
        var document = CreateValidDocument();

        document.AddLine(
            Quantity.Create(1m).Value, 59, "Item", Money.Create(100m).Value, Money.Zero,
            Money.Zero, Money.Zero, Money.Create(100m).Value, Money.Create(13m).Value);

        document.RemoveLine(1);

        document.Lines.Should().HaveCount(0);
        document.Totals.Should().Be(DocumentTotals.Zero);
    }

    [Fact]
    public void Totals_ShouldDeriveCorrectly_WithMixedTaxCategories()
    {
        var document = CreateValidDocument();

        // Line 1: Exempt only
        document.AddLine(
            Quantity.Create(1m).Value, 59, "Exempt item",
            Money.Create(50m).Value, Money.Zero,
            Money.Zero, Money.Create(50m).Value, Money.Zero, Money.Zero);

        // Line 2: Taxable only
        document.AddLine(
            Quantity.Create(1m).Value, 59, "Taxable item",
            Money.Create(100m).Value, Money.Zero,
            Money.Zero, Money.Zero, Money.Create(100m).Value, Money.Create(13m).Value);

        // Line 3: Non-taxable only
        document.AddLine(
            Quantity.Create(1m).Value, 59, "Non-taxable item",
            Money.Create(30m).Value, Money.Zero,
            Money.Create(30m).Value, Money.Zero, Money.Zero, Money.Zero);

        document.Totals.TotalNonTaxable.Amount.Should().Be(30m);
        document.Totals.TotalExempt.Amount.Should().Be(50m);
        document.Totals.TotalTaxable.Amount.Should().Be(100m);
        document.Totals.SubTotalSales.Amount.Should().Be(180m);
        document.Totals.TotalDiscount.Amount.Should().Be(0m);
        document.Totals.TotalTax.Amount.Should().Be(13m);
        document.Totals.TotalToPay.Amount.Should().Be(193m); // 180 + 13 - 0
    }

    private static FiscalDocument CreateValidDocument()
    {
        return FiscalDocument.Create(
            DocumentId.New(), DteType.FacturaElectronica, 2, EnvironmentType.Test, OperationType.Normal,
            DateOnly.FromDateTime(DateTime.UtcNow), TimeOnly.FromDateTime(DateTime.UtcNow),
            ValidIssuer(), null, DateTime.UtcNow).Value;
    }
}

