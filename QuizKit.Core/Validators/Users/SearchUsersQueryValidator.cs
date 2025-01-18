using FluentValidation;
using QuizKit.Common.Requests.Users;

namespace QuizKit.Core.Validators.Users;

public class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersQueryValidator()
    {
        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .When(x => x.MaxResults.HasValue)
            .WithMessage("Maximum results must be greater than 0");
    }
}
