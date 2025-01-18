using JwtFactory;
using MediatR;
using Microsoft.AspNetCore.Http;
using QuizKit.Common.Constants;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Entities;
using QuizKit.Core.Options;
using QuizKit.Core.RequestHandlers.Users;

namespace QuizKit.IntegrationTests;

public class LoginTestUserBehavior(
    IHttpContextAccessor httpcontextAccessor,
    JwtProvider jwtProvider,
    UserPolicyOptions userPolicy)
        : LoginHandlerBase(httpcontextAccessor, jwtProvider, userPolicy),
          IPipelineBehavior<LoginCommand, Result<LoggedInUserModel>>

{
    public const string adminUserEmail = "admin-user@quizkit.com";
    public const string basicUserEmail = "basic-user@spacecredit.com";

    public async Task<Result<LoggedInUserModel>> Handle(LoginCommand request, RequestHandlerDelegate<Result<LoggedInUserModel>> next, CancellationToken cancellationToken)
    {
        var testUser = request.Email switch
        {
            adminUserEmail => AdminUser,
            basicUserEmail => BasicUser,
            _ => default,
        };

        if (testUser == default)
        {
            return await next();
        }

        return Result.Success(await CreateLogin(testUser));
    }

    private static UserProfile AdminUser => CreateTestUser(adminUserEmail, "Admin", "Tester", Roles.Admin);

    private static UserProfile BasicUser => CreateTestUser(basicUserEmail, "Basic", "Tester", Roles.User);

    private static UserProfile CreateTestUser(string email, string firstName, string lastName, string role)
    {
        var user = new UserProfile(new()
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
        });
        user.AddRole(role);

        return user;
    }
}

