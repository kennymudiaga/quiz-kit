using FluentValidation;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Options;

namespace QuizKit.Core.Validators.Users;

public class SetPasswordComandValidator: AbstractValidator<SetPasswordCommand>
{
    public SetPasswordComandValidator(UserPolicyOptions userPolicyOptions)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Password).NotEmpty()
            .MinimumLength(userPolicyOptions.MinPasswordLength).WithMessage($"Password must be at least {userPolicyOptions.MinPasswordLength} characters")
            .MaximumLength(userPolicyOptions.MaxPasswordLength).WithMessage($"Password must be at most {userPolicyOptions.MaxPasswordLength} characters");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
    }
}
