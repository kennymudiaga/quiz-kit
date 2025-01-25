using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Validators.Quizzes;

namespace QuizKit.Tests.Validators.Quizzes;

public class GetQuizQueryValidatorTests
{
    private readonly GetQuizQueryValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyId_ShouldHaveError(string id)
    {
        // Arrange
        var query = new GetQuizQuery { Id = id };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_IdTooLong_ShouldHaveError()
    {
        // Arrange
        var query = new GetQuizQuery { Id = new string('a', 101) };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_ValidId_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetQuizQuery { Id = "valid-id" };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
