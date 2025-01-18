using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Options;

namespace QuizKit.Core.RequestHandlers.Users;

public class SetPasswordCommandHandler(
    QuizDbContext dbContext,
    IPasswordHasher<string> passwordHasher,
    UserPolicyOptions userPolicyOptions,
    ILogger<SetPasswordCommandHandler> logger
) : IRequestHandler<SetPasswordCommand, Result>
{
    private readonly QuizDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;

    public async Task<Result> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Find user by email and token
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.PasswordTokenHash))
        {
            // User not found or password reset not requested: return success to prevent enumeration attacks
            // TODO: Implement alert or mitigation (e.g. rate limiting) to prevent abuse
            logger.LogCritical("Suspicious password request {request}", request);
            return Result.Success();
        }

        if(user.IsPasswordTokenExpired)
        {
            // Token is not expired: return success to prevent enumeration attacks
            return Result.BadRequest("Token is expired");
        }

        //validate passwordTokenHash
        var verificationResult = _passwordHasher.VerifyHashedPassword(user.Email!, user.PasswordTokenHash, request.Token);
        if (verificationResult != PasswordVerificationResult.Success)
        {
            user.LogAccessFailure(userPolicyOptions.EnableLockout, userPolicyOptions.MaxPasswordFailCount, userPolicyOptions.PasswordLockoutDuration);
            return Result.BadRequest("Invalid token");
        }

        // Set the new password
        user.SetPassword(_passwordHasher.HashPassword(user.Email!, request.Password));

        // Save changes
        _dbContext.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
