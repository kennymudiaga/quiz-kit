using FluentValidation;
using QuizKit.Common.Requests.Questions;

namespace QuizKit.Core.Validators.Questions;

public class GetQuestionsQueryValidator : AbstractValidator<GetQuestionsQuery>
{
    public GetQuestionsQueryValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty()
            .MaximumLength(36)
            .WithMessage("Quiz ID is required and must not exceed 36 characters");
    }
}
