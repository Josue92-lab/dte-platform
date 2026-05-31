using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class DepartmentCodeTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var result = DepartmentCode.Create("06");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("06");
    }

    [Theory]
    [InlineData("01")]
    [InlineData("14")]
    public void Create_ShouldReturnSuccess_WhenValueIsTwoCharacters(string value)
    {
        var result = DepartmentCode.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenValueIsEmpty(string? value)
    {
        var result = DepartmentCode.Create(value!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DepartmentCode.Empty");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    public void Create_ShouldReturnFailure_WhenValueLengthIsInvalid(string value)
    {
        var result = DepartmentCode.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DepartmentCode.InvalidLength");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenValuesAreIdentical()
    {
        var a = DepartmentCode.Create("06").Value;
        var b = DepartmentCode.Create("06").Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        var a = DepartmentCode.Create("06").Value;
        var b = DepartmentCode.Create("14").Value;

        a.Should().NotBe(b);
    }
}
