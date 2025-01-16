using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public class SendInvitationCommand : IRequest<Result>
{
    public required string Email { get; init; }
    public required string Role { get; init; }
    public required string OrganizationId { get; init; }
}
