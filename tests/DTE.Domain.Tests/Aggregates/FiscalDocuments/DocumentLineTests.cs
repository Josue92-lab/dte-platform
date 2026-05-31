using DTE.Domain.Aggregates.FiscalDocuments;
using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.Aggregates.FiscalDocuments;

public class DocumentLineTests
{
    [Fact]
    public void AddLine_ShouldConstructAndReturnValidDocumentLine()
    {
        var issuer = IssuerSnapshot.Create(
            TaxIdentifier.Create("NIT", "06141804941035").Value,
            TaxIdentifier.Create("NRC", "1234567").Value,
            PartyName.Create("Empresa").Value,
            EconomicActivityCode.Create("46100", "Actividad").Value,
            Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Complemento").Value,
            PhoneNumber.Create("22001234").Value,
            EmailAddress.Create("info@empresa.com").Value).Value;

        var documentId = DocumentId.New();
        var document = FiscalDocument.Create(
            documentId, DteType.FacturaElectronica, 2, EnvironmentType.Test, OperationType.Normal,
            DateOnly.FromDateTime(DateTime.UtcNow), TimeOnly.FromDateTime(DateTime.UtcNow),
            issuer, null, DateTime.UtcNow).Value;

        var result = document.AddLine(
            Quantity.Create(2m).Value,
            59,
            "Producto de prueba",
            Money.Create(50m).Value,
            Money.Create(5m).Value,
            Money.Create(0m).Value,
            Money.Create(0m).Value,
            Money.Create(95m).Value,
            Money.Create(12.35m).Value);

        result.IsSuccess.Should().BeTrue();

        var line = result.Value;
        line.NumItem.Should().Be(1);
        line.Quantity.Value.Should().Be(2m);
        line.UnitOfMeasure.Should().Be(59);
        line.Description.Should().Be("Producto de prueba");
        line.UnitPrice.Amount.Should().Be(50m);
        line.DiscountAmount.Amount.Should().Be(5m);
        line.TaxableAmount.Amount.Should().Be(95m);
        line.TaxAmount.Amount.Should().Be(12.35m);
    }
}
