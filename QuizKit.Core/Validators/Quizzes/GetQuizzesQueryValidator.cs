using FluentValidation;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Constants;

namespace QuizKit.Core.Validators.Quizzes;

public class GetQuizzesQueryValidator : AbstractValidator<GetQuizzesQuery>
{
    public GetQuizzesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(PaginationConstants.MaxPageSize)
            .WithMessage($"Page size cannot be greater than {PaginationConstants.MaxPageSize}");
    }
}
