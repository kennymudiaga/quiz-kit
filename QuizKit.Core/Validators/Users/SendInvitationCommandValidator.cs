using FluentValidation;
using QuizKit.Common.Requests.Users;

namespace QuizKit.Core.Validators.Users;

public class SendInvitationCommandValidator : AbstractValidator<SendInvitationCommand>
{
    public SendInvitationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Role)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
