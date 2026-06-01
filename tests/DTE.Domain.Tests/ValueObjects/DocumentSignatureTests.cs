using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class DocumentSignatureTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValueIsValid()
    {
        var result = DocumentSignature.Create("abc123signaturehash");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("abc123signaturehash");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsNull()
    {
        var result = DocumentSignature.Create(null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentSignature.Empty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsEmpty()
    {
        var result = DocumentSignature.Create(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentSignature.Empty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsWhitespace()
    {
        var result = DocumentSignature.Create("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentSignature.Empty");
    }

    [Fact]
    public void Equality_ShouldBeTrue_WhenValuesAreEqual()
    {
        var a = DocumentSignature.Create("sig1").Value;
        var b = DocumentSignature.Create("sig1").Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenValuesAreDifferent()
    {
        var a = DocumentSignature.Create("sig1").Value;
        var b = DocumentSignature.Create("sig2").Value;

        a.Should().NotBe(b);
    }
}
