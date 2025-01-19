using FluentValidation;
using QuizKit.Common.Requests.Quizzes;

namespace QuizKit.Core.Validators.Quizzes;

public class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Title is required and must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.TimeLimit)
            .GreaterThan(0)
            .When(x => x.TimeLimit.HasValue)
            .WithMessage("Time limit must be greater than 0 minutes");

        RuleFor(x => x.OrganizationId)
            .MaximumLength(100)
            .WithMessage("Organization ID must not exceed 100 characters");
    }
}
