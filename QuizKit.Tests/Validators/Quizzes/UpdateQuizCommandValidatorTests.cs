using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Validators.Quizzes;
using Xunit;

namespace QuizKit.Tests.Validators.Quizzes;

public class UpdateQuizCommandValidatorTests
{
    private readonly UpdateQuizCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyId_ShouldHaveError(string id)
    {
        // Arrange
        var command = new UpdateQuizCommand { Id = id };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validate_IdTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand { Id = new string('a', 101) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyTitle_ShouldHaveError(string title)
    {
        // Arrange
        var command = new UpdateQuizCommand { Title = title };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_TitleTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand { Title = new string('a', 201) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand { Description = new string('a', 2001) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_InvalidTimeLimit_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand { TimeLimit = 0 };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TimeLimit);
    }

    [Fact]
    public async Task Validate_OrganizationIdTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand { OrganizationId = new string('a', 101) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrganizationId);
    }

    [Fact]
    public async Task Validate_CategoryIdTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand { CategoryId = new string('a', 101) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public async Task Validate_ImageUrlTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand { ImageUrl = new string('a', 2001) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public async Task Validate_EndDateBeforeStartDate_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand
        {
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public async Task Validate_ValidCommand_ShouldNotHaveError()
    {
        // Arrange
        var command = new UpdateQuizCommand
        {
            Id = "valid-id",
            Title = "Valid Title",
            Description = "Valid Description",
            TimeLimit = 30,
            OrganizationId = "org-id",
            CategoryId = "cat-id",
            ImageUrl = "http://example.com/image.jpg",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
