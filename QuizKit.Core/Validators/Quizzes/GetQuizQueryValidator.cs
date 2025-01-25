using FluentValidation;
using QuizKit.Common.Requests.Quizzes;

namespace QuizKit.Core.Validators.Quizzes;

public class GetQuizQueryValidator : AbstractValidator<GetQuizQuery>
{
    public GetQuizQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Quiz ID is required and must not exceed 100 characters");
    }
}
