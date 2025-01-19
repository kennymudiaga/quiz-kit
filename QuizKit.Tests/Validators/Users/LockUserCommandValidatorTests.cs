using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Validators.Users;
using Xunit;

namespace QuizKit.Tests.Validators.Users;

public class LockUserCommandValidatorTests
{
    private readonly LockUserCommandValidator _validator;

    public LockUserCommandValidatorTests()
    {
        _validator = new LockUserCommandValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyEmail_ShouldHaveError(string email)
    {
        // Arrange
        var command = new LockUserCommand { UserId = email };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Validate_NullId_ShouldHaveError()
    {
        // Arrange
        var command = new LockUserCommand();
        // Act
        var result = await _validator.TestValidateAsync(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Validate_LongerThan200Id_ShouldHaveError()
    {
        // Arrange
        var command = new LockUserCommand { UserId = new string('a', 201) };
        // Act
        var result = await _validator.TestValidateAsync(command);
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Validate_ValidUserId_ShouldNotHaveError()
    {
        // Arrange
        var command = new LockUserCommand { UserId = "test@example.com" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("a")]
    public async Task Validate_InvalidReason_ShouldHaveError(string reason)
    {
        // Arrange
        var command = new LockUserCommand { Reason = reason };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public async Task Validate_ValidReason_ShouldNotHaveError()
    {
        // Arrange
        var command = new LockUserCommand { Reason = "Valid reason for locking" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public async Task Validate_PastLockoutExpiry_ShouldHaveError()
    {
        // Arrange
        var command = new LockUserCommand { LockoutExpiry = DateTime.UtcNow.AddMinutes(-1) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LockoutExpiry);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(24)]
    [InlineData(168)]
    public async Task Validate_FutureLockoutExpiry_ShouldNotHaveError(int hoursInFuture)
    {
        // Arrange
        var command = new LockUserCommand { LockoutExpiry = DateTime.UtcNow.AddHours(hoursInFuture) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LockoutExpiry);
    }

    [Fact]
    public async Task Validate_NullLockoutExpiry_ShouldNotHaveError()
    {
        // Arrange
        var command = new LockUserCommand { LockoutExpiry = null };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LockoutExpiry);
    }
}
