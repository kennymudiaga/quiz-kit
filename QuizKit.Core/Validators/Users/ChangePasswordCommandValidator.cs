using FluentValidation;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Options;

namespace QuizKit.Core.Validators.Users;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator(UserPolicyOptions userPolicyOptions)
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password")
            .MinimumLength(userPolicyOptions.MinPasswordLength).WithMessage($"New password must be at least {userPolicyOptions.MinPasswordLength} characters")
            .MaximumLength(userPolicyOptions.MaxPasswordLength).WithMessage($"New password must be at most {userPolicyOptions.MaxPasswordLength} characters");
    }
}
