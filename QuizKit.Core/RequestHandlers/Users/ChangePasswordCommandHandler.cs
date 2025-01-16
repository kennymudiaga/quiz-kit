using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Options;
using System.Security.Claims;

namespace QuizKit.Core.RequestHandlers.Users;

public class ChangePasswordCommandHandler(
    QuizDbContext dbContext,
    IPasswordHasher<string> passwordHasher,
    UserPolicyOptions userPolicyOptions,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ChangePasswordCommandHandler> logger
) : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly QuizDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;
    private readonly UserPolicyOptions _userPolicyOptions = userPolicyOptions;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<ChangePasswordCommandHandler> _logger = logger;

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Get user email from claims
        var email = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Attempt to change password without email claim");
            return new Failure("User email not found", ResultStatus.Unauthorized);
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user == null)
        {
            // If this happens, there's a bug or security issue. A non-existing user should not be logged in.
            // Log this as a critical error
            _logger.LogCritical("Logged-in user '{email}' not found in user store!", email);
            return new Failure("User not found", ResultStatus.NotFound);
        }

        // Check if user account is locked
        if (user.IsAccountLocked)
        {
            _logger.LogError("Locked user '{email}' attempted to change password", email);
            return new Failure("Account is locked. Cannot change password.", ResultStatus.Forbidden);
        }

        var result = default(Result);

        // Verify current password
        var verificationResult = _passwordHasher.VerifyHashedPassword(user.Email!, user.PasswordHash!, request.CurrentPassword!);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            // If password fails, it should count as a failed login attempt
            result = new Failure("Current password is incorrect", ResultStatus.BadRequest);
        }
        else
        {
            // Hash new password
            user.SetPassword(_passwordHasher.HashPassword(user.Email!, request.NewPassword!));
        }

        _dbContext.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result ?? Result.Success();
    }
}
