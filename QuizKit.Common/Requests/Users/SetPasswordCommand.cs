using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public record SetPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = "";
    public string Token { get; init; } = "";
    public string Password { get; init; } = "";
    public string ConfirmPassword { get; init; } = "";
}
