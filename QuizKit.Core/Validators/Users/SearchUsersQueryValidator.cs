using FluentValidation;
using QuizKit.Common.Requests.Users;

namespace QuizKit.Core.Validators.Users;

public class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    private const int MaxPageSize = 100;

    public SearchUsersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(MaxPageSize)
            .WithMessage($"Page size cannot be greater than {MaxPageSize}");
    }
}
