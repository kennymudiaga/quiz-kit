using QuizKit.Common.Requests.Users;
using QuizKit.Core.Validators.Users;

namespace QuizKit.Tests.Validators;

public class SignUpCommandValidatorTests
{
    private readonly SignUpCommandValidator validator = new();
    private readonly SignUpCommand OkCommand = new()
    {
        ConfirmPassword = "password",
        Email = "mail@test.com",
        FirstName = "John",
        LastName = "Doe",
        Password = "password",
        PhoneNumber = "07021231234"
    };

    [Fact]
    public void ShouldPassValidation()
    {
        var result = validator.Validate(OkCommand);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void InvalidEmail_ShouldFailValidation()
    {
        var command = OkCommand with { Email = "mail" };
        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("_123757575")] // Invalid characters.
    [InlineData("1237575753")] // No international code, doesn't start with 0.
    [InlineData("0809123123463233")] // Too long.
    [InlineData("+234809123123413")] // Too long.
    [InlineData("+23480")] // Too few digits after international code.
    [InlineData("080212")] // Too few digits after initial 0.
    public void InvalidPhoneNumber_ShouldFailValidation(string phoneNumber)
    {
        var command = OkCommand with { PhoneNumber = phoneNumber };
        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void PasswordMismatch_ShouldFailValidation()
    {
        var command = OkCommand with { ConfirmPassword = "password1" };

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "ConfirmPassword");
    }

    [Fact]
    public void PasswordTooShort_ShouldFailValidation()
    {
        var command = OkCommand with
        {
            Password = "1234567",
        };

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Password");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyFirstName_ShouldFailValidation(string? name)
    {
        var command = OkCommand with { FirstName = name };
        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "FirstName");
    }

    [Fact]
    public void FirstNameTooLong_ShouldFailValidation()
    {
        var name = Enumerable.Range(0, 31).Select(x => "a").Aggregate((x, y) => x + y);
        var command = OkCommand with
        {
            FirstName = name,
        };

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "FirstName");
    }

    [Fact]
    public void LastNameTooLong_ShouldFailValidation()
    {
        var name = Enumerable.Range(0, 31).Select(x => "a").Aggregate((x, y) => x + y);
        var command = OkCommand with
        {
            LastName = name,
        };

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "LastName");
    }

    [Fact]
    public void InviteCodeTooLong_ShouldFailValidation()
    {
        var code = Enumerable.Range(0, 9).Select(x => "a").Aggregate((x, y) => x + y);
        var command = OkCommand with
        {
            InviteCode = code,
        };

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "InviteCode");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Invalid invite code.");
    }
}
