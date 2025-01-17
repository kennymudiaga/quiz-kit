using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Validators.Users;

namespace QuizKit.Tests.Validators.Users;

public class RequestPasswordResetCommandValidatorTests
{
    private readonly RequestPasswordResetCommandValidator _validator;

    public RequestPasswordResetCommandValidatorTests()
    {
        _validator = new RequestPasswordResetCommandValidator();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RequestPasswordResetCommand { Email = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithNullOrWhitespaceEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new RequestPasswordResetCommand { Email = email };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("invalid.email")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new RequestPasswordResetCommand { Email = email };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Invalid email format.");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("firstname+lastname@domain.com")]
    public void Validate_WithValidEmail_ShouldNotHaveValidationErrors(string email)
    {
        // Arrange
        var command = new RequestPasswordResetCommand { Email = email };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
