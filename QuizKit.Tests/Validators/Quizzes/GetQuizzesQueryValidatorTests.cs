using FluentValidation.TestHelper;
using QuizKit.Common.Constants;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Validators.Quizzes;

namespace QuizKit.Tests.Validators.Quizzes;

public class GetQuizzesQueryValidatorTests
{
    private readonly GetQuizzesQueryValidator _validator;

    public GetQuizzesQueryValidatorTests()
    {
        _validator = new GetQuizzesQueryValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WhenPageSizeIsLessThanOne_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var query = new GetQuizzesQuery { PageSize = pageSize };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
              .WithErrorMessage("Page size must be greater than 0");
    }

    [Fact]
    public async Task Validate_WhenPageSizeExceedsMaximum_ShouldHaveValidationError()
    {
        // Arrange
        var query = new GetQuizzesQuery { PageSize = PaginationConstants.MaxPageSize + 1 };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
              .WithErrorMessage($"Page size cannot be greater than {PaginationConstants.MaxPageSize}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(250)]
    public async Task Validate_WhenPageSizeIsValid_ShouldNotHaveValidationError(int pageSize)
    {
        // Arrange
        var query = new GetQuizzesQuery { PageSize = pageSize };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }
}
