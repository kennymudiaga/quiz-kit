using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Options;
using JwtFactory;

namespace QuizKit.Core.RequestHandlers.Users;

public class LoginCommandHandler(
    QuizDbContext dbContext,
    IPasswordHasher<string> passwordHasher,
    IHttpContextAccessor httpContextAccessor,
    JwtProvider jwtProvider,
    UserPolicyOptions userPolicyOptions
) : LoginHandlerBase(httpContextAccessor, jwtProvider, userPolicyOptions), IRequestHandler<LoginCommand, Result<LoggedInUserModel>>
{
    private readonly QuizDbContext _dbContext = dbContext;
    private readonly IPasswordHasher<string> _passwordHasher = passwordHasher;

    public async Task<Result<LoggedInUserModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return new Failure("Invalid email or password", ResultStatus.BadRequest);
        }

        if (user.IsAccountLocked)
        {
            return new Failure("Account is locked. Please try again later.", ResultStatus.BadRequest);
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user.Email!, user.PasswordHash!, request.Password!);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            user.LogAccessFailure(UserPolicy.EnableLockout, UserPolicy.MaxPasswordFailCount, UserPolicy.PasswordLockoutDuration);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new Failure("Invalid email or password", ResultStatus.BadRequest);
        }

        user.LogAccessSuccess();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(await CreateLogin(user));
    }
}
