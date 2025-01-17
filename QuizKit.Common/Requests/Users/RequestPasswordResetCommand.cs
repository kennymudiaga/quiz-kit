using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public record RequestPasswordResetCommand : IRequest<Result>
{
    public string Email { get; init; } = null!;
}
