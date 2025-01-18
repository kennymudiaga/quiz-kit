using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Options;
using QuizKit.Core.Validators.Users;

namespace QuizKit.Tests.Validators.Users;

public class SetPasswordCommandValidatorTests
{
    private readonly SetPasswordCommandValidator _validator;
    private readonly UserPolicyOptions _userPolicyOptions;

    public SetPasswordCommandValidatorTests()
    {
        _userPolicyOptions = new UserPolicyOptions
        {
            MinPasswordLength = 8,
            MaxPasswordLength = 50
        };
        _validator = new SetPasswordCommandValidator(_userPolicyOptions);
    }

    [Fact]
    public void ShouldPassValidation_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new SetPasswordCommand
        {
            Email = "test@example.com",
            Token = "validToken123",
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Token123", "Password123!", "Password123!")]
    [InlineData(null, "Token123", "Password123!", "Password123!")]
    [InlineData("invalid-email", "Token123", "Password123!", "Password123!")]
    public void ShouldFailValidation_WhenEmailIsInvalid(string email, string token, string password, string confirmPassword)
    {
        // Arrange
        var command = new SetPasswordCommand
        {
            Email = email,
            Token = token,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("test@example.com", "", "Password123!", "Password123!")]
    [InlineData("test@example.com", null, "Password123!", "Password123!")]
    public void ShouldFailValidation_WhenTokenIsEmpty(string email, string token, string password, string confirmPassword)
    {
        // Arrange
        var command = new SetPasswordCommand
        {
            Email = email,
            Token = token,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Theory]
    [InlineData("test@example.com", "Token123", "", "")]
    [InlineData("test@example.com", "Token123", null, null)]
    [InlineData("test@example.com", "Token123", "short", "short")]
    [InlineData("test@example.com", "Token123", "verylongpasswordthatexceedsthemaximumlengthallowedbythevalidator", "verylongpasswordthatexceedsthemaximumlengthallowedbythevalidator")]
    public void ShouldFailValidation_WhenPasswordIsInvalid(string email, string token, string password, string confirmPassword)
    {
        // Arrange
        var command = new SetPasswordCommand
        {
            Email = email,
            Token = token,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ShouldFailValidation_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var command = new SetPasswordCommand
        {
            Email = "test@example.com",
            Token = "Token123",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}
