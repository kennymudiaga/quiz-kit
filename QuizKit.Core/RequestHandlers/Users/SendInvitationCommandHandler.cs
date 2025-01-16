using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.Options;

namespace QuizKit.Core.RequestHandlers.Users;

public class SendInvitationCommandHandler(
    QuizDbContext dbContext,
    IPasswordHasher<string> passwordHasher,
    UserPolicyOptions userPolicyOptions
) : IRequestHandler<SendInvitationCommand, Result>
{
    private readonly QuizDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;
    private readonly UserPolicyOptions _userPolicyOptions = userPolicyOptions;

    public async Task<Result> Handle(SendInvitationCommand request, CancellationToken cancellationToken)
    {
        // Check if organization exists
        if (!await _dbContext.Organizations.AnyAsync(x => x.Id == request.OrganizationId, cancellationToken))
        {
            return new Failure("Organization not found", ResultStatus.NotFound);
        }

        // Check if user is already in the organization
        var existingUser = await _dbContext.Users
            .Include(UserProfile.OrganizationsNavigationName)
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (existingUser?.Organizations.Any(x => x.OrganizationId == request.OrganizationId) == true)
        {
            return new Failure("User is already a member of this organization", ResultStatus.BadRequest);
        }

        // Check if there's already a pending invitation
        var existingInvitation = await _dbContext.Invitations
            .FirstOrDefaultAsync(x => x.Email == request.Email && 
                                    x.OrganizationId == request.OrganizationId && 
                                    x.Status == "Pending", 
                                    cancellationToken);

        if (existingInvitation != null)
        {
            // Extend the invitation expiry
            existingInvitation.ExpiryDate = DateTime.UtcNow.AddHours(_userPolicyOptions.InviteExpiryHours);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        // Generate invite code and create invitation
        var inviteCode = Guid.NewGuid().ToString("N");
        var invitation = new Invitation
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            OrganizationId = request.OrganizationId,
            Role = request.Role,
            TokenHash = _passwordHasher.HashPassword(request.Email, inviteCode),
            Status = "Pending",
            ExpiryDate = DateTime.UtcNow.AddHours(_userPolicyOptions.InviteExpiryHours)
        };

        _dbContext.Invitations.Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // TODO: Send invitation email with the invite code

        return Result.Success();
    }
}
