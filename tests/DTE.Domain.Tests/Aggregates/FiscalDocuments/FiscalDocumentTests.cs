using DTE.Domain.Aggregates.FiscalDocuments;
using DTE.Domain.Events;
using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.Aggregates.FiscalDocuments;

public class FiscalDocumentTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValid()
    {
        var documentId = DocumentId.New();
        var dteType = DteType.FacturaElectronica;
        var now = DateTime.UtcNow;
        var issueDate = DateOnly.FromDateTime(now);
        var issueTime = TimeOnly.FromDateTime(now);

        var result = FiscalDocument.Create(documentId, dteType, 2, EnvironmentType.Test, OperationType.Normal, issueDate, issueTime, now);

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

        var domainEvent = document.GetDomainEvents().SingleOrDefault() as FiscalDocumentCreated;
        domainEvent.Should().NotBeNull();
        domainEvent!.DocumentId.Should().Be(documentId);
        domainEvent.DteType.Should().Be(dteType);
        domainEvent.DocumentVersion.Should().Be(2);
        domainEvent.EnvironmentType.Should().Be(EnvironmentType.Test);
        domainEvent.OperationType.Should().Be(OperationType.Normal);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDocumentIdIsNull()
    {
        var dteType = DteType.FacturaElectronica;
        var result = FiscalDocument.Create(null!, dteType, 2, EnvironmentType.Test, OperationType.Normal, DateOnly.MinValue, TimeOnly.MinValue, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.DocumentIdNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDteTypeIsNull()
    {
        var documentId = DocumentId.New();
        var result = FiscalDocument.Create(documentId, null!, 2, EnvironmentType.Test, OperationType.Normal, DateOnly.MinValue, TimeOnly.MinValue, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.DteTypeNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDocumentVersionIsInvalid()
    {
        var documentId = DocumentId.New();
        var result = FiscalDocument.Create(documentId, DteType.FacturaElectronica, 0, EnvironmentType.Test, OperationType.Normal, DateOnly.MinValue, TimeOnly.MinValue, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.InvalidDocumentVersion");
    }
}
