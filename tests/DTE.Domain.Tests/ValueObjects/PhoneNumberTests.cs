using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+503 2222-2222")]
    [InlineData("22222222")]
    public void Create_ShouldReturnSuccess_WhenFormatIsValid(string phone)
    {
        var result = PhoneNumber.Create(phone);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("123")] // too short
    [InlineData("invalid-letters")]
    public void Create_ShouldReturnFailure_WhenFormatIsInvalid(string phone)
    {
        var result = PhoneNumber.Create(phone);
        result.IsFailure.Should().BeTrue();
    }
}
