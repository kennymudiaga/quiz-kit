using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizKit.Common.Enums;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Options;
using QuizKit.Core.ServiceContracts;

namespace QuizKit.Core.RequestHandlers.Users;

public class RequestPasswordResetCommandHandler(
    QuizDbContext dbContext,
    UserPolicyOptions userPolicyOptions,
    ITokenGenerator tokenGenerator,
    IPasswordHasher<string> passwordHasher,
    ILogger<RequestPasswordResetCommandHandler> logger
) : IRequestHandler<RequestPasswordResetCommand, Result>
{
    private readonly QuizDbContext _dbContext = dbContext;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger = logger;

    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            // Log as information to prevent email enumeration
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
            return Result.Success(); // Always return success to prevent email enumeration
        }

        if (user.IsAccountLocked)
        {
            _logger.LogWarning("Password reset attempted for locked account: {Email}", request.Email);
            return new Failure("Account is locked", ResultStatus.Forbidden);
        }

        try
        {
            // Generate an 8-digit numeric code
            var resetCode = _tokenGenerator.GenerateToken(8, TokenType.Numeric);
            var tokenHash = passwordHasher.HashPassword(user.Email!, resetCode);
            // Set the password reset token and expiration
            user.SetPasswordToken(tokenHash, userPolicyOptions.PasswordTokenTimeout);

            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // TODO: Implement email sending service to send reset code to user
            _logger.LogInformation("Password reset code generated for user: {Email}", user.Email);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating password reset code for user: {Email}", user.Email);
            return new Failure("Unable to process password reset", ResultStatus.Failure);
        }
    }
}
