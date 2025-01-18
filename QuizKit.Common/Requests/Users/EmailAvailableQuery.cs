using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public record EmailAvailableQuery : IRequest<Result>
{
    public required string Email { get; init; }
}
