using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public record ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
}
