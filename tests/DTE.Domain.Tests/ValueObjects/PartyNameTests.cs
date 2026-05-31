using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class PartyNameTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenLegalNameIsValid()
    {
        var result = PartyName.Create("Empresa Ejemplo S.A. de C.V.");

        result.IsSuccess.Should().BeTrue();
        result.Value.LegalName.Should().Be("Empresa Ejemplo S.A. de C.V.");
        result.Value.CommercialName.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenCommercialNameIsProvided()
    {
        var result = PartyName.Create("Empresa Ejemplo S.A. de C.V.", "Mi Tienda");

        result.IsSuccess.Should().BeTrue();
        result.Value.LegalName.Should().Be("Empresa Ejemplo S.A. de C.V.");
        result.Value.CommercialName.Should().Be("Mi Tienda");
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenLegalNameIsAtMaxLength()
    {
        var legalName = new string('A', 250);
        var result = PartyName.Create(legalName);

        result.IsSuccess.Should().BeTrue();
        result.Value.LegalName.Length.Should().Be(250);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenCommercialNameIsAtMaxLength()
    {
        var commercialName = new string('B', 150);
        var result = PartyName.Create("Valid Name", commercialName);

        result.IsSuccess.Should().BeTrue();
        result.Value.CommercialName!.Length.Should().Be(150);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenLegalNameIsEmpty(string? legalName)
    {
        var result = PartyName.Create(legalName!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PartyName.LegalNameEmpty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenLegalNameExceedsMaxLength()
    {
        var legalName = new string('A', 251);
        var result = PartyName.Create(legalName);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PartyName.LegalNameTooLong");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCommercialNameExceedsMaxLength()
    {
        var commercialName = new string('B', 151);
        var result = PartyName.Create("Valid Name", commercialName);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PartyName.CommercialNameTooLong");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenValuesAreIdentical()
    {
        var a = PartyName.Create("Legal", "Commercial").Value;
        var b = PartyName.Create("Legal", "Commercial").Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        var a = PartyName.Create("Legal A").Value;
        var b = PartyName.Create("Legal B").Value;

        a.Should().NotBe(b);
    }
}
