using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Validators.Quizzes;
using Xunit;

namespace QuizKit.Tests.Validators.Quizzes;

public class DeleteQuizCommandValidatorTests
{
    private readonly DeleteQuizCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyId_ShouldHaveError(string id)
    {
        // Arrange
        var command = new DeleteQuizCommand { Id = id };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_IdTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new DeleteQuizCommand { Id = new string('a', 101) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_ValidId_ShouldNotHaveError()
    {
        // Arrange
        var command = new DeleteQuizCommand { Id = "valid-id" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
