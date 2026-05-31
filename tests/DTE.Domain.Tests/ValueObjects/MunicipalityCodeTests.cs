using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class MunicipalityCodeTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var result = MunicipalityCode.Create("14");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("14");
    }

    [Theory]
    [InlineData("01")]
    [InlineData("22")]
    public void Create_ShouldReturnSuccess_WhenValueIsTwoCharacters(string value)
    {
        var result = MunicipalityCode.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenValueIsEmpty(string? value)
    {
        var result = MunicipalityCode.Create(value!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MunicipalityCode.Empty");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    public void Create_ShouldReturnFailure_WhenValueLengthIsInvalid(string value)
    {
        var result = MunicipalityCode.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MunicipalityCode.InvalidLength");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenValuesAreIdentical()
    {
        var a = MunicipalityCode.Create("14").Value;
        var b = MunicipalityCode.Create("14").Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        var a = MunicipalityCode.Create("14").Value;
        var b = MunicipalityCode.Create("22").Value;

        a.Should().NotBe(b);
    }
}
