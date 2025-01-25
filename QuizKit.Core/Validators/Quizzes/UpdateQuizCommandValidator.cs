using FluentValidation;
using QuizKit.Common.Requests.Quizzes;

namespace QuizKit.Core.Validators.Quizzes;

public class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Quiz ID is required and must not exceed 100 characters");

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

        RuleFor(x => x.CategoryId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.CategoryId))
            .WithMessage("Category ID must not exceed 100 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Image URL must not exceed 2000 characters");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}
