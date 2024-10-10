using FluentValidation;
using QuizKit.Common.Requests.Users;

namespace QuizKit.Core.Validators.Users;

public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty().MaximumLength(30);

        RuleFor(x => x.LastName)
            .NotEmpty().MaximumLength(30);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^(0|\+)[1-9]\d{5,13}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.InviteCode)
            .MaximumLength(8).WithMessage("Invalid invite code.");
    }
}
