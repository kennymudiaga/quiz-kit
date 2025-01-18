using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Validators.Users;

namespace QuizKit.Tests.Validators.Users;

public class EmailAvailableQueryValidatorTests
{
    private readonly EmailAvailableQueryValidator _validator;

    public EmailAvailableQueryValidatorTests()
    {
        _validator = new EmailAvailableQueryValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyEmail_ShouldFail(string email)
    {
        // Arrange
        var query = new EmailAvailableQuery { Email = email };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    public async Task Validate_WithInvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var query = new EmailAvailableQuery { Email = email };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("valid@example.com")]
    [InlineData("test.user@domain.com")]
    [InlineData("user+tag@example.co.uk")]
    public async Task Validate_WithValidEmail_ShouldPass(string email)
    {
        // Arrange
        var query = new EmailAvailableQuery { Email = email };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
