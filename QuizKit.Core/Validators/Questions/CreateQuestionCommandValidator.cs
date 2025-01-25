using FluentValidation;
using QuizKit.Common.Requests.Questions;

namespace QuizKit.Core.Validators.Questions;

public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Quiz ID is required and must not exceed 100 characters");

        RuleFor(x => x.QuestionText)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("Question text is required and must not exceed 1000 characters");

        RuleFor(x => x.A)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Option A is required and must not exceed 500 characters");

        RuleFor(x => x.B)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Option B is required and must not exceed 500 characters");

        RuleFor(x => x.C)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Option C is required and must not exceed 500 characters");

        RuleFor(x => x.D)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Option D is required and must not exceed 500 characters");

        RuleFor(x => x.Answer)
            .NotEmpty()
            .Must(answer => new[] { "A", "B", "C", "D" }.Contains(answer?.ToUpper()))
            .WithMessage("Answer must be one of: A, B, C, or D");
    }
}
