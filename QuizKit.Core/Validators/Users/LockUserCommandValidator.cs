namespace QuizKit.Core.Validators.Users;

using FluentValidation;
using QuizKit.Common.Requests.Users;

public class LockUserCommandValidator : AbstractValidator<LockUserCommand>
{
    public LockUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .MaximumLength(36);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(200)
            .WithMessage("A reason between 5 and 200 characters is required");

        RuleFor(x => x.LockoutExpiry)
            .GreaterThanOrEqualTo(DateTime.UtcNow).WithMessage("Lockout expiry must be in the future.");
    }
}
