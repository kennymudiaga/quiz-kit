using MediatR;
using QuizKit.Common.Models.Users;

namespace QuizKit.Common.Requests.Users;

public record SignUpCommand : IRequest<LoggedInUserModel>
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? PhoneNumber { get; set; }
    public string? InviteCode { get; set; }
}
