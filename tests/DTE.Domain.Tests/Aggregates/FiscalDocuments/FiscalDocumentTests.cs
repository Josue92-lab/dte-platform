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
}
