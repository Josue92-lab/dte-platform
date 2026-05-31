using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class DteTypeTests
{
    [Theory]
    [InlineData("01", "FE")]
    [InlineData("03", "CCF")]
    public void FromCode_ShouldReturnSuccess_WhenCodeIsValid(string code, string expectedDescription)
    {
        var result = DteType.FromCode(code);
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Be(expectedDescription);
    }

    [Fact]
    public void FromCode_ShouldReturnFailure_WhenCodeIsInvalid()
    {
        var result = DteType.FromCode("99");
        result.IsFailure.Should().BeTrue();
    }
}
