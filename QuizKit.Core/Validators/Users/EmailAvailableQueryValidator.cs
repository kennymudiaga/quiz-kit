using FluentValidation;
using QuizKit.Common.Requests.Users;

namespace QuizKit.Core.Validators.Users;

public class EmailAvailableQueryValidator : AbstractValidator<EmailAvailableQuery>
{
    public EmailAvailableQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Please provide a valid email address");
    }
}
