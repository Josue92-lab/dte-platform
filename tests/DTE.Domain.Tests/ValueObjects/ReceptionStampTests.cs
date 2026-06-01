using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class ReceptionStampTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var result = ReceptionStamp.Create("2026ABCDEF123456SELLO");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("2026ABCDEF123456SELLO");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsNull()
    {
        var result = ReceptionStamp.Create(null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ReceptionStamp.Empty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsEmpty()
    {
        var result = ReceptionStamp.Create(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ReceptionStamp.Empty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsWhitespace()
    {
        var result = ReceptionStamp.Create("  ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ReceptionStamp.Empty");
    }

    [Fact]
    public void Equality_ShouldBeTrue_WhenValuesAreEqual()
    {
        var a = ReceptionStamp.Create("sello1").Value;
        var b = ReceptionStamp.Create("sello1").Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenValuesAreDifferent()
    {
        var a = ReceptionStamp.Create("sello1").Value;
        var b = ReceptionStamp.Create("sello2").Value;

        a.Should().NotBe(b);
    }
}
