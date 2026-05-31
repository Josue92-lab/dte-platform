using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class EconomicActivityCodeTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenCodeAndDescriptionAreValid()
    {
        var result = EconomicActivityCode.Create("46100", "Comercio al por mayor");

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("46100");
        result.Value.Description.Should().Be("Comercio al por mayor");
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenCodeIs5Characters()
    {
        var result = EconomicActivityCode.Create("46100", "Valid Description");

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Length.Should().Be(5);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenCodeIs6Characters()
    {
        var result = EconomicActivityCode.Create("461001", "Valid Description");

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Length.Should().Be(6);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDescriptionIsAtMinLength()
    {
        var result = EconomicActivityCode.Create("46100", "ABCDE");

        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Length.Should().Be(5);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDescriptionIsAtMaxLength()
    {
        var description = new string('X', 150);
        var result = EconomicActivityCode.Create("46100", description);

        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Length.Should().Be(150);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenCodeIsEmpty(string? code)
    {
        var result = EconomicActivityCode.Create(code!, "Valid Description");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EconomicActivityCode.CodeEmpty");
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("1234567")]
    public void Create_ShouldReturnFailure_WhenCodeLengthIsInvalid(string code)
    {
        var result = EconomicActivityCode.Create(code, "Valid Description");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EconomicActivityCode.CodeInvalidLength");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenDescriptionIsEmpty(string? description)
    {
        var result = EconomicActivityCode.Create("46100", description!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EconomicActivityCode.DescriptionEmpty");
    }

    [Theory]
    [InlineData("ABCD")]
    public void Create_ShouldReturnFailure_WhenDescriptionIsTooShort(string description)
    {
        var result = EconomicActivityCode.Create("46100", description);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EconomicActivityCode.DescriptionInvalidLength");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDescriptionExceedsMaxLength()
    {
        var description = new string('X', 151);
        var result = EconomicActivityCode.Create("46100", description);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EconomicActivityCode.DescriptionInvalidLength");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenValuesAreIdentical()
    {
        var a = EconomicActivityCode.Create("46100", "Comercio").Value;
        var b = EconomicActivityCode.Create("46100", "Comercio").Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenCodesAreDifferent()
    {
        var a = EconomicActivityCode.Create("46100", "Comercio").Value;
        var b = EconomicActivityCode.Create("46200", "Comercio").Value;

        a.Should().NotBe(b);
    }
}
