using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class AddressTests
{
    private static DepartmentCode ValidDepartment() => DepartmentCode.Create("06").Value;
    private static MunicipalityCode ValidMunicipality() => MunicipalityCode.Create("14").Value;

    [Fact]
    public void Create_ShouldReturnSuccess_WhenAllComponentsAreValid()
    {
        var result = Address.Create(ValidDepartment(), ValidMunicipality(), "Colonia Escalón, Calle Principal #123");

        result.IsSuccess.Should().BeTrue();
        result.Value.Department.Value.Should().Be("06");
        result.Value.Municipality.Value.Should().Be("14");
        result.Value.Complement.Should().Be("Colonia Escalón, Calle Principal #123");
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenComplementIsAtMaxLength()
    {
        var complement = new string('C', 200);
        var result = Address.Create(ValidDepartment(), ValidMunicipality(), complement);

        result.IsSuccess.Should().BeTrue();
        result.Value.Complement.Length.Should().Be(200);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDepartmentIsNull()
    {
        var result = Address.Create(null!, ValidMunicipality(), "Valid Complement");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.DepartmentNull");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenMunicipalityIsNull()
    {
        var result = Address.Create(ValidDepartment(), null!, "Valid Complement");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.MunicipalityNull");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenComplementIsEmpty(string? complement)
    {
        var result = Address.Create(ValidDepartment(), ValidMunicipality(), complement!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.ComplementEmpty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenComplementExceedsMaxLength()
    {
        var complement = new string('C', 201);
        var result = Address.Create(ValidDepartment(), ValidMunicipality(), complement);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Address.ComplementTooLong");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenValuesAreIdentical()
    {
        var a = Address.Create(ValidDepartment(), ValidMunicipality(), "Same Address").Value;
        var b = Address.Create(ValidDepartment(), ValidMunicipality(), "Same Address").Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComplementsDiffer()
    {
        var a = Address.Create(ValidDepartment(), ValidMunicipality(), "Address A").Value;
        var b = Address.Create(ValidDepartment(), ValidMunicipality(), "Address B").Value;

        a.Should().NotBe(b);
    }
}
