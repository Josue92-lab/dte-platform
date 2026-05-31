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

        var result = FiscalDocument.Create(documentId, dteType, now);

        result.IsSuccess.Should().BeTrue();
        
        var document = result.Value;
        document.DocumentId.Should().Be(documentId);
        document.DteType.Should().Be(dteType);
        document.Status.Should().Be(FiscalDocumentStatus.Draft);
        document.ControlNumber.Should().BeNull();
        document.GenerationCode.Should().BeNull();
        document.CreatedAtUtc.Should().Be(now);

        var domainEvent = document.GetDomainEvents().SingleOrDefault() as FiscalDocumentCreated;
        domainEvent.Should().NotBeNull();
        domainEvent!.DocumentId.Should().Be(documentId);
        domainEvent.DteType.Should().Be(dteType);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDocumentIdIsNull()
    {
        var dteType = DteType.FacturaElectronica;
        var result = FiscalDocument.Create(null!, dteType, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.DocumentIdNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDteTypeIsNull()
    {
        var documentId = DocumentId.New();
        var result = FiscalDocument.Create(documentId, null!, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FiscalDocument.DteTypeNull");
    }
}
