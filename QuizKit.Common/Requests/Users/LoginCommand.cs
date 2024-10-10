using MediatR;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public record LoginCommand : IRequest<Result<LoggedInUserModel>>
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
