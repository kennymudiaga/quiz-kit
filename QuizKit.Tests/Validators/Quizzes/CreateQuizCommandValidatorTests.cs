using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Validators.Quizzes;
using Xunit;

namespace QuizKit.Tests.Validators.Quizzes;

public class CreateQuizCommandValidatorTests
{
    private readonly CreateQuizCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmptyTitle_ShouldHaveError(string title)
    {
        // Arrange
        var command = new CreateQuizCommand { Title = title };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_TitleTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new CreateQuizCommand { Title = new string('a', 201) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_ValidTitle_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateQuizCommand { Title = "Valid Title" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new CreateQuizCommand { Description = new string('a', 2001) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_ValidDescription_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateQuizCommand { Description = "Valid Description" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidTimeLimit_ShouldHaveError(int timeLimit)
    {
        // Arrange
        var command = new CreateQuizCommand { TimeLimit = timeLimit };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TimeLimit);
    }

    [Fact]
    public async Task Validate_ValidTimeLimit_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateQuizCommand { TimeLimit = 30 };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TimeLimit);
    }

    [Fact]
    public async Task Validate_OrganizationIdTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new CreateQuizCommand { OrganizationId = new string('a', 101) };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrganizationId);
    }

    [Fact]
    public async Task Validate_ValidOrganizationId_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateQuizCommand { OrganizationId = "valid-org-id" };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OrganizationId);
    }
}
