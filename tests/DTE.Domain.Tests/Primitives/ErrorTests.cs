using DTE.Domain.Primitives;
using FluentAssertions;

namespace DTE.Domain.Tests.Primitives;

public class ErrorTests
{
    [Fact]
    public void None_ShouldHaveEmptyCodeAndMessage()
    {
        // Assert
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetCodeAndMessage()
    {
        // Arrange
        var code = "Test.Code";
        var message = "Test message";

        // Act
        var error = new Error(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
    }
}
