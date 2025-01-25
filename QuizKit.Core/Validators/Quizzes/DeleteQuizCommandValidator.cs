using FluentValidation;
using QuizKit.Common.Requests.Quizzes;

namespace QuizKit.Core.Validators.Quizzes;

public class DeleteQuizCommandValidator : AbstractValidator<DeleteQuizCommand>
{
    public DeleteQuizCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Quiz ID is required and must not exceed 100 characters");
    }
}
