using MediatR;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using Microsoft.EntityFrameworkCore;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using JwtFactory;
using QuizKit.Core.Options;

namespace QuizKit.Core.RequestHandlers.Users;

public class SignUpCommandHandler(
    QuizDbContext dbContext,
    IPasswordHasher<string> passwordHasher,
    IHttpContextAccessor httpContextAccessor,
    JwtProvider jwtProvider,
    UserPolicyOptions userPolicyOptions
) : LoginHandlerBase(httpContextAccessor, jwtProvider, userPolicyOptions), IRequestHandler<SignUpCommand, Result<LoggedInUserModel>>
{
    private readonly QuizDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;

    public async Task<Result<LoggedInUserModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        if (await _dbContext.Users.AnyAsync(x => x.Email == request.Email, cancellationToken))
        {
            return new Failure("Email is already registered", ResultStatus.BadRequest);
        }

        // Create user profile
        var user = new UserProfile(request);
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.SetPassword(_passwordHasher.HashPassword(user.Email!, request.Password));
        }
        _dbContext.Add(user);

        if (!string.IsNullOrWhiteSpace(request.InviteCode))
        {
            // Check if there's a pending invitation
            var invitation = await _dbContext.Invitations
                .FirstOrDefaultAsync(x => x.Email == request.Email && x.TokenHash == request.InviteCode && x.Status == "Pending", cancellationToken);

            // If user was invited, add them to the organization
            if (invitation != null)
            {
                user.AddOrganization(invitation.OrganizationId!, invitation.Role!);
                
                // Mark invitation as used
                invitation.Accept();
                _dbContext.Update(invitation);
            }
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(await CreateLogin(user));
    }
}
