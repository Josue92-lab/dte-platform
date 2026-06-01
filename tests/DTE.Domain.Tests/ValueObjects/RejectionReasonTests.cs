using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class RejectionReasonTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValid()
    {
        var result = RejectionReason.Create("MH-001", "Invalid document structure.");

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("MH-001");
        result.Value.Description.Should().Be("Invalid document structure.");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCodeIsNull()
    {
        var result = RejectionReason.Create(null!, "Some description.");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RejectionReason.CodeEmpty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCodeIsEmpty()
    {
        var result = RejectionReason.Create(string.Empty, "Some description.");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RejectionReason.CodeEmpty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCodeIsWhitespace()
    {
        var result = RejectionReason.Create("  ", "Some description.");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RejectionReason.CodeEmpty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDescriptionIsNull()
    {
        var result = RejectionReason.Create("MH-001", null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RejectionReason.DescriptionEmpty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDescriptionIsEmpty()
    {
        var result = RejectionReason.Create("MH-001", string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RejectionReason.DescriptionEmpty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDescriptionIsWhitespace()
    {
        var result = RejectionReason.Create("MH-001", "   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RejectionReason.DescriptionEmpty");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDescriptionExceedsMaxLength()
    {
        var longDescription = new string('A', 501);
        var result = RejectionReason.Create("MH-001", longDescription);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("RejectionReason.DescriptionTooLong");
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDescriptionIsExactlyMaxLength()
    {
        var description = new string('A', 500);
        var result = RejectionReason.Create("MH-001", description);

        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Length.Should().Be(500);
    }

    [Fact]
    public void Equality_ShouldBeTrue_WhenBothFieldsAreEqual()
    {
        var a = RejectionReason.Create("MH-001", "Desc").Value;
        var b = RejectionReason.Create("MH-001", "Desc").Value;

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_ShouldBeFalse_WhenCodesDiffer()
    {
        var a = RejectionReason.Create("MH-001", "Desc").Value;
        var b = RejectionReason.Create("MH-002", "Desc").Value;

        a.Should().NotBe(b);
    }
}
