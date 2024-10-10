using MediatR;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public record InviteUserCommand : IRequest<Result<InvitationModel>>
{
    public string? Email { get; set; }
    public string? OrganizationId { get; set; }
    public string? Role { get; set; }
}
