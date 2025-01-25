using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Questions;
using QuizKit.Core.Validators.Questions;

namespace QuizKit.Tests.Validators.Questions;

public class UpdateQuestionCommandValidatorTests
{
    private readonly UpdateQuestionCommandValidator _validator = new();


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyQuizId_ShouldHaveError(string quizId)
    {
        // Arrange
        var command = new UpdateQuestionCommand
        {
            Id = "1",
            QuizId = quizId,
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuizId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyQuestionText_ShouldHaveError(string questionText)
    {
        // Arrange
        var command = new UpdateQuestionCommand
        {
            Id = "1",
            QuizId = "test-quiz",
            QuestionText = questionText,
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuestionText);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyOptions_ShouldHaveError(string option)
    {
        // Arrange
        var command = new UpdateQuestionCommand
        {
            Id = "1",
            QuizId = "test-quiz",
            QuestionText = "Test Question",
            A = option,
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.A);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("E")]
    [InlineData("X")]
    public async Task Validate_InvalidAnswer_ShouldHaveError(string answer)
    {
        // Arrange
        var command = new UpdateQuestionCommand
        {
            Id = "1",
            QuizId = "test-quiz",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = answer
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Answer);
    }

    [Fact]
    public async Task Validate_ValidCommand_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateQuestionCommand
        {
            Id = "1",
            QuizId = "test-quiz",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
