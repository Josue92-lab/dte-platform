using DTE.Domain.Primitives;
using FluentAssertions;

namespace DTE.Domain.Tests.Primitives;

public class ValueObjectTests
{
    private sealed class TestValueObject : ValueObject
    {
        public string Value1 { get; }
        public int Value2 { get; }

        public TestValueObject(string value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value1;
            yield return Value2;
        }
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenAllValuesAreEqual()
    {
        // Arrange
        var value1 = new TestValueObject("Test", 1);
        var value2 = new TestValueObject("Test", 1);

        // Act & Assert
        value1.Equals(value2).Should().BeTrue();
        (value1 == value2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        // Arrange
        var value1 = new TestValueObject("Test", 1);
        var value2 = new TestValueObject("Test2", 1);

        // Act & Assert
        value1.Equals(value2).Should().BeFalse();
        (value1 != value2).Should().BeTrue();
    }
}
