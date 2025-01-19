namespace QuizKit.Core.Validators.Users;

using FluentValidation;
using QuizKit.Common.Requests.Users;

public class LockUserCommandValidator : AbstractValidator<LockUserCommand>
{
    public LockUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(200)
            .WithMessage("A reason between 5 and 200 characters is required");

        RuleFor(x => x.LockoutExpiry)
            .Must(x => !x.HasValue || x.Value >= DateTime.UtcNow)
            .WithMessage("Lockout expiry must be in the future.");
    }
}
