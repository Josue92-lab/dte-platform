using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class TaxIdentifierTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenKindAndValueAreValid()
    {
        var result = TaxIdentifier.Create("NIT", "0614-010190-101-1");
        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be("NIT");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsEmpty()
    {
        var result = TaxIdentifier.Create("NIT", "");
        result.IsFailure.Should().BeTrue();
    }
}
