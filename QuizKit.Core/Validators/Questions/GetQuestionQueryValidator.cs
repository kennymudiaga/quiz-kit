using FluentValidation;
using QuizKit.Common.Requests.Questions;

namespace QuizKit.Core.Validators.Questions;

public class GetQuestionQueryValidator : AbstractValidator<GetQuestionQuery>
{
    public GetQuestionQueryValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty()
            .MaximumLength(36)
            .WithMessage("Quiz ID is required and must not exceed 36 characters");

        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(36)
            .WithMessage("Question ID is required and must not exceed 36 characters");
    }
}
