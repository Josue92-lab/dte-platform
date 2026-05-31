using DTE.Domain.Primitives;
using FluentAssertions;

namespace DTE.Domain.Tests.Primitives;

public class ValidationResultTests
{
    [Fact]
    public void Valid_ShouldReturnIsValidTrue_WhenNoErrorsAdded()
    {
        var result = ValidationResult.Valid();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenSingleErrorAdded()
    {
        var result = new ValidationResult();
        result.AddError(new ValidationError("Test.Error", "A test error."));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Errors_ShouldAccumulateMultipleErrors()
    {
        var result = new ValidationResult();
        result.AddError(new ValidationError("Error.One", "First error."));
        result.AddError(new ValidationError("Error.Two", "Second error."));
        result.AddError(new ValidationError("Error.Three", "Third error."));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void ValidationError_ShouldPreserveAllProperties()
    {
        var error = new ValidationError("Test.Code", "Test message.", "Test.Field");

        error.Code.Should().Be("Test.Code");
        error.Message.Should().Be("Test message.");
        error.Field.Should().Be("Test.Field");
    }

    [Fact]
    public void ValidationError_ShouldDefaultFieldToNull()
    {
        var error = new ValidationError("Test.Code", "Test message.");

        error.Field.Should().BeNull();
    }
}
