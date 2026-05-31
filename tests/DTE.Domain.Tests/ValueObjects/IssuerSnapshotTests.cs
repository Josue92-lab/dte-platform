using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class IssuerSnapshotTests
{
    private static TaxIdentifier ValidNit() => TaxIdentifier.Create("NIT", "06141804941035").Value;
    private static TaxIdentifier ValidNrc() => TaxIdentifier.Create("NRC", "1234567").Value;
    private static PartyName ValidName() => PartyName.Create("Empresa Ejemplo S.A. de C.V.", "Mi Tienda").Value;
    private static EconomicActivityCode ValidActivity() => EconomicActivityCode.Create("46100", "Comercio al por mayor").Value;
    private static Address ValidAddress() => Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Colonia Escalón #123").Value;
    private static PhoneNumber ValidPhone() => PhoneNumber.Create("22001234").Value;
    private static EmailAddress ValidEmail() => EmailAddress.Create("info@empresa.com").Value;

    [Fact]
    public void Create_ShouldReturnSuccess_WhenAllFieldsAreValid()
    {
        var result = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), ValidName(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail());

        result.IsSuccess.Should().BeTrue();
        result.Value.Nit.Value.Should().Be("06141804941035");
        result.Value.Nrc.Value.Should().Be("1234567");
        result.Value.Name.LegalName.Should().Be("Empresa Ejemplo S.A. de C.V.");
        result.Value.EconomicActivity.Code.Should().Be("46100");
        result.Value.Address.Department.Value.Should().Be("06");
        result.Value.Phone.Value.Should().Be("22001234");
        result.Value.Email.Value.Should().Be("info@empresa.com");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNitIsNull()
    {
        var result = IssuerSnapshot.Create(
            null!, ValidNrc(), ValidName(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuerSnapshot.NitNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNrcIsNull()
    {
        var result = IssuerSnapshot.Create(
            ValidNit(), null!, ValidName(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuerSnapshot.NrcNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNameIsNull()
    {
        var result = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), null!, ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuerSnapshot.NameNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenEconomicActivityIsNull()
    {
        var result = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), ValidName(), null!, ValidAddress(), ValidPhone(), ValidEmail());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuerSnapshot.EconomicActivityNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenAddressIsNull()
    {
        var result = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), ValidName(), ValidActivity(), null!, ValidPhone(), ValidEmail());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuerSnapshot.AddressNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenPhoneIsNull()
    {
        var result = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), ValidName(), ValidActivity(), ValidAddress(), null!, ValidEmail());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuerSnapshot.PhoneNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenEmailIsNull()
    {
        var result = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), ValidName(), ValidActivity(), ValidAddress(), ValidPhone(), null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuerSnapshot.EmailNull");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenAllFieldsAreIdentical()
    {
        var a = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), ValidName(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail()).Value;
        var b = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), ValidName(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail()).Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenAnyFieldDiffers()
    {
        var a = IssuerSnapshot.Create(
            ValidNit(), ValidNrc(), ValidName(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail()).Value;
        var differentNit = TaxIdentifier.Create("NIT", "99999999999999").Value;
        var b = IssuerSnapshot.Create(
            differentNit, ValidNrc(), ValidName(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail()).Value;

        a.Should().NotBe(b);
    }
}
