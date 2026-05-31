using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class DocumentIdTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenGuidIsValid()
    {
        var result = DocumentId.Create(Guid.NewGuid());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenGuidIsEmpty()
    {
        var result = DocumentId.Create(Guid.Empty);
        result.IsFailure.Should().BeTrue();
    }
}
