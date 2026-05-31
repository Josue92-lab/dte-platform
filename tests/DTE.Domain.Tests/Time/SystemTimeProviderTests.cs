using DTE.Domain.Time;
using FluentAssertions;

namespace DTE.Domain.Tests.Time;

public class SystemTimeProviderTests
{
    [Fact]
    public void UtcNow_ShouldReturnCurrentDateTimeUtc()
    {
        // Arrange
        var provider = new SystemTimeProvider();

        // Act
        var result = provider.UtcNow;

        // Assert
        result.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
