using QuizKit.Common.Requests.Users;
using QuizKit.Core.Entities;

namespace QuizKit.Tests.TestHelpers;

public static class TestUser
{
    public static UserProfile Create(string email, bool isLocked = false)
    {
        var user = new UserProfile(new SignUpCommand
        {
            Email = email,
            FirstName = "Test",
            LastName = "User"
        });

        if (isLocked)
        {
            user.LogAccessFailure(true, 3, 30);
            user.LogAccessFailure(true, 3, 30);
            user.LogAccessFailure(true, 3, 30);
        }

        return user;
    }
}
