using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Validators.Users;
using Xunit;

namespace QuizKit.Tests.Validators.Users;

public class UnlockUserCommandValidatorTests
{
    private readonly UnlockUserCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyId_ShouldHaveError(string id)
    {
        // Arrange
        var command = new UnlockUserCommand { UserId = id };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Validate_NullId_ShouldHaveError()
    {
        // Arrange
        var command = new UnlockUserCommand();
        // Act
        var result = await _validator.TestValidateAsync(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Validate_LongerThan36Id_ShouldHaveError()
    {
        // Arrange
        var command = new UnlockUserCommand { UserId = new string('a', 37) };
        // Act
        var result = await _validator.TestValidateAsync(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Validate_ValidUserId_ShouldNotHaveError()
    {
        // Arrange
        var command = new UnlockUserCommand { UserId = "valid-id" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyReason_ShouldHaveError(string reason)
    {
        // Arrange
        var command = new UnlockUserCommand { Reason = reason };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public async Task Validate_LongerThan200Reason_ShouldHaveError()
    {
        // Arrange
        var command = new UnlockUserCommand { Reason = new string('a', 201) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public async Task Validate_ValidReason_ShouldNotHaveError()
    {
        // Arrange
        var command = new UnlockUserCommand { Reason = "Valid reason" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }
}
