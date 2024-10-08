using MediatR;
using QuizKit.Common.Models.Users;

namespace QuizKit.Common.Requests.Users;

public record InviteUserCommand : IRequest<InvitationModel>
{
    public string? Email { get; set; }
    public string? OrganizationId { get; set; }
    public string? Role { get; set; }
}
