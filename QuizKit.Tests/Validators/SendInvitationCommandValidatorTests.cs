using FluentValidation.TestHelper;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Validators.Users;

namespace QuizKit.Tests.Validators;

public class SendInvitationCommandValidatorTests
{
    private readonly SendInvitationCommandValidator _validator;

    public SendInvitationCommandValidatorTests()
    {
        _validator = new SendInvitationCommandValidator();
    }

    [Fact]
    public void ShouldPassValidation()
    {
        var command = new SendInvitationCommand
        {
            Email = "test@example.com",
            OrganizationId = "org123",
            Role = "Member"
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    public void InvalidEmail_ShouldFailValidation(string email)
    {
        var command = new SendInvitationCommand
        {
            Email = email,
            OrganizationId = "org123",
            Role = "Member"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyRole_ShouldFailValidation(string role)
    {
        var command = new SendInvitationCommand
        {
            Email = "test@example.com",
            OrganizationId = "org123",
            Role = role
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyOrganizationId_ShouldFailValidation(string organizationId)
    {
        var command = new SendInvitationCommand
        {
            Email = "test@example.com",
            OrganizationId = organizationId,
            Role = "Member"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrganizationId);
    }
}
