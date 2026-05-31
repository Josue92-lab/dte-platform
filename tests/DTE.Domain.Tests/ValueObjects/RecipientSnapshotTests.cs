using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class RecipientSnapshotTests
{
    private static PartyName ValidName() => PartyName.Create("Consumidor Final").Value;
    private static TaxIdentifier ValidDocId() => TaxIdentifier.Create("NIT", "01234567890123").Value;
    private static TaxIdentifier ValidNrc() => TaxIdentifier.Create("NRC", "7654321").Value;
    private static EconomicActivityCode ValidActivity() => EconomicActivityCode.Create("46200", "Comercio al por menor").Value;
    private static Address ValidAddress() => Address.Create(DepartmentCode.Create("06").Value, MunicipalityCode.Create("14").Value, "Colonia San Benito #456").Value;
    private static PhoneNumber ValidPhone() => PhoneNumber.Create("77001234").Value;
    private static EmailAddress ValidEmail() => EmailAddress.Create("cliente@correo.com").Value;

    [Fact]
    public void Create_ShouldReturnSuccess_WhenOnlyNameIsProvided()
    {
        var result = RecipientSnapshot.Create(ValidName());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.LegalName.Should().Be("Consumidor Final");
        result.Value.DocumentIdentifier.Should().BeNull();
        result.Value.Nrc.Should().BeNull();
        result.Value.EconomicActivity.Should().BeNull();
        result.Value.Address.Should().BeNull();
        result.Value.Phone.Should().BeNull();
        result.Value.Email.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenAllFieldsAreProvided()
    {
        var result = RecipientSnapshot.Create(
            ValidName(), ValidDocId(), ValidNrc(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.LegalName.Should().Be("Consumidor Final");
        result.Value.DocumentIdentifier!.Value.Should().Be("01234567890123");
        result.Value.Nrc!.Value.Should().Be("7654321");
        result.Value.EconomicActivity!.Code.Should().Be("46200");
        result.Value.Address!.Department.Value.Should().Be("06");
        result.Value.Phone!.Value.Should().Be("77001234");
        result.Value.Email!.Value.Should().Be("cliente@correo.com");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNameIsNull()
    {
        var result = RecipientSnapshot.Create(null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RecipientSnapshot.NameNull");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenAllFieldsAreIdentical()
    {
        var a = RecipientSnapshot.Create(
            ValidName(), ValidDocId(), ValidNrc(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail()).Value;
        var b = RecipientSnapshot.Create(
            ValidName(), ValidDocId(), ValidNrc(), ValidActivity(), ValidAddress(), ValidPhone(), ValidEmail()).Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenBothHaveOnlyName()
    {
        var a = RecipientSnapshot.Create(ValidName()).Value;
        var b = RecipientSnapshot.Create(ValidName()).Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenNamesDiffer()
    {
        var a = RecipientSnapshot.Create(ValidName()).Value;
        var b = RecipientSnapshot.Create(PartyName.Create("Otro Cliente").Value).Value;

        a.Should().NotBe(b);
    }
}
