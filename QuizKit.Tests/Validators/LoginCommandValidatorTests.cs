using QuizKit.Common.Requests.Users;
using QuizKit.Core.Validators.Users;

namespace QuizKit.Tests.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator validator = new();
    private readonly LoginCommand OkCommand = new()
    {
        Email = "test@example.com",
        Password = "password123"
    };

    [Fact]
    public void ShouldPassValidation()
    {
        var result = validator.Validate(OkCommand);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void InvalidEmail_ShouldFailValidation(string email)
    {
        var command = OkCommand with { Email = email };
        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void EmptyPassword_ShouldFailValidation(string password)
    {
        var command = OkCommand with { Password = password };
        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Password");
    }
}
