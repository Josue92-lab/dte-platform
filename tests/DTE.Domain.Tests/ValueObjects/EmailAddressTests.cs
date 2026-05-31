using DTE.Domain.ValueObjects;
using FluentAssertions;

namespace DTE.Domain.Tests.ValueObjects;

public class EmailAddressTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    public void Create_ShouldReturnSuccess_WhenFormatIsValid(string email)
    {
        var result = EmailAddress.Create(email);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@missingusername.com")]
    public void Create_ShouldReturnFailure_WhenFormatIsInvalid(string email)
    {
        var result = EmailAddress.Create(email);
        result.IsFailure.Should().BeTrue();
    }
}
