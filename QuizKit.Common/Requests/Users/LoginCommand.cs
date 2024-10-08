using MediatR;
using QuizKit.Common.Models.Users;

namespace QuizKit.Common.Requests.Users;

public record LoginCommand : IRequest<LoggedInUserModel>
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
