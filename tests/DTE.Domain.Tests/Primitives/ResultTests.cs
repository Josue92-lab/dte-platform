using DTE.Domain.Primitives;
using FluentAssertions;

namespace DTE.Domain.Tests.Primitives;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnIsSuccessTrue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldReturnIsFailureTrue()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void SuccessT_ShouldReturnIsSuccessTrueAndValue()
    {
        // Act
        var result = Result.Success("Value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Value");
    }

    [Fact]
    public void FailureT_AccessingValue_ShouldThrowException()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result.Failure<string>(error);

        // Act
        var action = () => { var _ = result.Value; };

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
