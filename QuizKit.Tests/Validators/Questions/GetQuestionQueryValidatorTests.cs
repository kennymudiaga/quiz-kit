using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Questions;
using QuizKit.Core.Validators.Questions;

namespace QuizKit.Tests.Validators.Questions;

public class GetQuestionQueryValidatorTests
{
    private readonly GetQuestionQueryValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyQuizId_ShouldHaveError(string quizId)
    {
        // Arrange
        var query = new GetQuestionQuery { QuizId = quizId, Id = "test-id" };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuizId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyId_ShouldHaveError(string id)
    {
        // Arrange
        var query = new GetQuestionQuery { QuizId = "test-quiz", Id = id };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_ExtraLongQuizId_ShouldHaveError()
    {
        // Arrange
        var query = new GetQuestionQuery 
        { 
            QuizId = new string('a', 37), // 37 characters
            Id = "test-id" 
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuizId)
            .WithErrorMessage("Quiz ID is required and must not exceed 36 characters");
    }

    [Fact]
    public async Task Validate_ExtraLongId_ShouldHaveError()
    {
        // Arrange
        var query = new GetQuestionQuery 
        { 
            QuizId = "test-quiz",
            Id = new string('a', 37) // 37 characters
        };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Question ID is required and must not exceed 36 characters");
    }

    [Fact]
    public async Task Validate_ValidQuery_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetQuestionQuery { QuizId = "test-quiz", Id = "test-id" };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
