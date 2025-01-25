using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Questions;
using QuizKit.Core.Validators.Questions;

namespace QuizKit.Tests.Validators.Questions;

public class GetQuestionsQueryValidatorTests
{
    private readonly GetQuestionsQueryValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyQuizId_ShouldHaveError(string quizId)
    {
        // Arrange
        var query = new GetQuestionsQuery { QuizId = quizId };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuizId);
    }

    [Fact]
    public async Task Validate_ValidQuery_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetQuestionsQuery { QuizId = "test-quiz" };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_TooLongQuizId_ShouldHaveError()
    {
        // Arrange
        var query = new GetQuestionsQuery { QuizId = new string('a', 37) };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuizId);
    }
}
