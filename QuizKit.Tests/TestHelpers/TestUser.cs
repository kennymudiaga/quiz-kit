using QuizKit.Common.Requests.Users;
using QuizKit.Core.Entities;

namespace QuizKit.Tests.TestHelpers;

public static class TestUser
{
    public static UserProfile Create(string email, bool isLocked = false, string firstName = "Test", string lastName = "User")
    {
        var user = new UserProfile(new SignUpCommand
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = "+1234567890",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123"
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
