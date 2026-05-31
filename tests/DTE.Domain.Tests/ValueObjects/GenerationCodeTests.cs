using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class GenerationCodeTests
{
    [Fact]
    public void Parse_ShouldReturnSuccess_WhenStringIsGuid()
    {
        var guid = Guid.NewGuid();
        var result = GenerationCode.Parse(guid.ToString());
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void Parse_ShouldReturnFailure_WhenStringIsInvalid()
    {
        var result = GenerationCode.Parse("not-a-guid");
        result.IsFailure.Should().BeTrue();
    }
}
