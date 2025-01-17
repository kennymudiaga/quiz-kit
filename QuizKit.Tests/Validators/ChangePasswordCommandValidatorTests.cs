using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Options;
using QuizKit.Core.Validators.Users;
using Xunit;

namespace QuizKit.Tests.Validators;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;
    private readonly UserPolicyOptions _userPolicyOptions;

    public ChangePasswordCommandValidatorTests()
    {
        _userPolicyOptions = new UserPolicyOptions
        {
            MinPasswordLength = 8,
            MaxPasswordLength = 64
        };
        _validator = new ChangePasswordCommandValidator(_userPolicyOptions);
    }

    [Fact]
    public void ShouldPassValidation_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "oldPassword123",
            NewPassword = "newPassword456"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "newPassword456", "CurrentPassword")]
    [InlineData("oldPassword123", "", "NewPassword")]
    public void ShouldFailValidation_WhenRequiredFieldsAreMissing(
        string currentPassword, 
        string newPassword, 
        string expectedPropertyName)
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(expectedPropertyName);
    }

    [Fact]
    public void ShouldFailValidation_WhenNewPasswordIsSameAsCurrentPassword()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "samePassword123",
            NewPassword = "samePassword123"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password must be different from current password");
    }

    [Theory]
    [InlineData("short", "New password must be at least 8 characters")]
    [InlineData("verylongpasswordthatexceedsthemaximumlengthofsixtyfourcharactersandshouldfail", 
        "New password must be at most 64 characters")]
    public void ShouldFailValidation_WhenPasswordLengthIsInvalid(
        string newPassword, 
        string expectedErrorMessage)
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "oldPassword123",
            NewPassword = newPassword
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(expectedErrorMessage);
    }
}
