using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class ControlNumberTests
{
    [Theory]
    [InlineData("DTE-01-M001P001-000000000000001")]
    [InlineData("dte-03-A1B2C3D4-123456789012345")]
    public void Create_ShouldReturnSuccess_WhenFormatIsValid(string value)
    {
        var result = ControlNumber.Create(value);
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("INVALID")]
    [InlineData("DTE-01-SHORT-000000000000001")]
    [InlineData("DTE-01-M001P001-123")]
    public void Create_ShouldReturnFailure_WhenFormatIsInvalid(string value)
    {
        var result = ControlNumber.Create(value);
        result.IsFailure.Should().BeTrue();
    }
}
