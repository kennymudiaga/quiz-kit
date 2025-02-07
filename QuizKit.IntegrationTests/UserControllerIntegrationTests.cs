using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using Xunit;
using QuizKit.IntegrationTests.TestBase;

namespace QuizKit.IntegrationTests;

public class UserControllerIntegrationTests : IntegrationTestBase
{
    public UserControllerIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SignUp_WithValidDetails_ReturnsCreatedUser()
    {
        // Arrange
        var signUpCommand = new SignUpCommand
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/signup", signUpCommand);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode,
            $"Expected success status code, but got {response.StatusCode}. " +
            $"Response content: {responseContent}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = JsonSerializer.Deserialize<LoggedInUserModel>(responseContent, JsonOptions);
        Assert.NotNull(content);

        // Check data directly instead of relying on IsSuccess
        if (content == null)
        {
            Assert.Fail(
                "Signup failed: Data is null. " +
                $"Full Response: {responseContent}"
            );
        }
        Assert.Equal(signUpCommand.Email, content.Email);
    }

    [Fact]
    public async Task SignUp_WithInvalidPhoneNumber_ReturnsBadRequest()
    {
        // Arrange
        var signUpCommand = new SignUpCommand
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "InvalidPhoneNumber"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/signup", signUpCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<Result>(responseContent, JsonOptions);
        Assert.NotNull(content);
        Assert.False(content.IsSuccess);
        Assert.Contains("validation", content.Message?.ToLowerInvariant() ?? string.Empty);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange: First sign up a user
        var signUpCommand = new SignUpCommand
        {
            Email = $"login{Guid.NewGuid()}@example.com",
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FirstName = "Login",
            LastName = "Test",
            PhoneNumber = "+1234567890"
        };

        var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
        var signUpResponseContent = await signUpResponse.Content.ReadAsStringAsync();
        var signUpContent = JsonSerializer.Deserialize<LoggedInUserModel>(
            signUpResponseContent,
            JsonOptions
        );
        Assert.True(signUpResponse.IsSuccessStatusCode,
            $"Signup failed. Full Response: {signUpResponseContent}, " +
            $"Status: {signUpResponse.StatusCode}");

        // Act: Now login with the same credentials
        var loginCommand = new LoginCommand
        {
            Email = signUpCommand.Email,
            Password = signUpCommand.Password
        };

        var loginResponse = await Client.PostAsJsonAsync("/user/login", loginCommand);

        // Assert
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

        Assert.True(loginResponse.IsSuccessStatusCode,
            $"Expected success status code, but got {loginResponse.StatusCode}. " +
            $"Response content: {loginResponseContent}");

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginContent = JsonSerializer.Deserialize<LoggedInUserModel>(loginResponseContent, JsonOptions);
        Assert.NotNull(loginContent);

        // Check data directly instead of relying on IsSuccess
        if (loginContent == null)
        {
            Assert.Fail(
                "Login failed: Data is null. " +
                $"Full Response: {loginResponseContent}"
            );
        }
        Assert.NotNull(loginContent.Token);
    }

    [Fact]
    public async Task ChangePassword_WithValidCredentials_Succeeds()
    {
        // Arrange: First sign up a user
        var signUpCommand = new SignUpCommand
        {
            Email = $"changepass{Guid.NewGuid()}@example.com",
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FirstName = "Change",
            LastName = "Password",
            PhoneNumber = "+1234567890"
        };

        var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
        var signUpContent = JsonSerializer.Deserialize<LoggedInUserModel>(
            await signUpResponse.Content.ReadAsStringAsync(),
            JsonOptions
        );
        Assert.True(signUpResponse.IsSuccessStatusCode,
            $"Signup failed. Status: {signUpResponse.StatusCode}, " +
            $"Content: {await signUpResponse.Content.ReadAsStringAsync()}");

        // Act: Login and then change password
        var loginCommand = new LoginCommand
        {
            Email = signUpCommand.Email,
            Password = signUpCommand.Password
        };

        var loginResponse = await Client.PostAsJsonAsync("/user/login", loginCommand);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = JsonSerializer.Deserialize<LoggedInUserModel>(
            await loginResponse.Content.ReadAsStringAsync(),
            JsonOptions
        );

        // Set the authorization token
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent!.Token);

        var changePasswordCommand = new ChangePasswordCommand
        {
            CurrentPassword = signUpCommand.Password,
            NewPassword = "NewStrongPassword456!"
        };

        var changePasswordResponse = await Client.PostAsJsonAsync("/user/change-password", changePasswordCommand);

        // Assert
        var changePasswordResponseContent = await changePasswordResponse.Content.ReadAsStringAsync();

        Assert.True(changePasswordResponse.IsSuccessStatusCode,
            $"Expected success status code, but got {changePasswordResponse.StatusCode}. " +
            $"Response content: {changePasswordResponseContent}");

        Assert.Equal(HttpStatusCode.NoContent, changePasswordResponse.StatusCode);

        // Verify new password works
        var newLoginCommand = new LoginCommand
        {
            Email = signUpCommand.Email,
            Password = changePasswordCommand.NewPassword
        };

        var newLoginResponse = await Client.PostAsJsonAsync("/user/login", newLoginCommand);
        newLoginResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginCommand = new LoginCommand
        {
            Email = $"invalid{Guid.NewGuid()}@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/login", loginCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<Result>(responseContent, JsonOptions);
        Assert.NotNull(content);
        Assert.False(content.IsSuccess);
        Assert.Contains("invalid", content.Message?.ToLowerInvariant() ?? string.Empty);
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var changePasswordCommand = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewStrongPassword456!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/change-password", changePasswordCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestPasswordReset_WithValidEmail_Succeeds()
    {
        // Arrange
        var email = "test@example.com";
        var signUpCommand = new SignUpCommand
        {
            Email = $"reset-paasword-{Guid.NewGuid()}@example.com",
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FirstName = "Change",
            LastName = "Password",
            PhoneNumber = "+1234567890"
        };
        var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
        signUpResponse.EnsureSuccessStatusCode();

        var requestPasswordResetCommand = new RequestPasswordResetCommand
        {
            Email = email
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/reset-password", requestPasswordResetCommand);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RequestPasswordReset_WithNonExistentEmail_Succeeds()
    {
        // Arrange
        var requestPasswordResetCommand = new RequestPasswordResetCommand
        {
            Email = "nonexistent@example.com"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/reset-password", requestPasswordResetCommand);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SetPassword_WithValidDetails_Succeeds()
    {
        // Arrange
        var email = $"set-password-{Guid.NewGuid()}@example.com";

        var signUpCommand = new SignUpCommand
        {
            Email = email,
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FirstName = "Change",
            LastName = "Password",
            PhoneNumber = "+1234567890"
        };
        var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
        signUpResponse.EnsureSuccessStatusCode();

        // Then, request a password reset        
        var resetCommand = new RequestPasswordResetCommand { Email = email };
        var resetResponse = await Client.PostAsJsonAsync("/user/reset-password", resetCommand);
        resetResponse.EnsureSuccessStatusCode();

        var token = "12345678";
        var newPassword = "NewPassword123!";
        // Set the new password
        var setPasswordCommand = new SetPasswordCommand
        {
            Email = email,
            Token = token,
            Password = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/set-password", setPasswordCommand);
        var text = await response.Content.ReadAsStringAsync();
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify login with new password
        var loginCommand = new LoginCommand
        {
            Email = email,
            Password = newPassword
        };
        var loginResponse = await Client.PostAsJsonAsync("/user/login", loginCommand);
        loginResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SetPassword_WithExpiredToken_Fails()
    {
        // Arrange
        var email = $"set-password-expired-{Guid.NewGuid()}@example.com";
        var oldPassword = "StrongPassword123!";
        var signUpCommand = new SignUpCommand
        {
            Email = email,
            Password = oldPassword,
            ConfirmPassword = oldPassword,
            FirstName = "Change",
            LastName = "Password",
            PhoneNumber = "+1234567890"
        };
        var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
        signUpResponse.EnsureSuccessStatusCode();

        // Then, request a password reset        
        var resetCommand = new RequestPasswordResetCommand { Email = email };
        var resetResponse = await Client.PostAsJsonAsync("/user/reset-password", resetCommand);
        resetResponse.EnsureSuccessStatusCode();

        //  Wait 20 seconds for the token to expire
        await Task.Delay(TimeSpan.FromSeconds(20));

        var token = "12345678";
        var newPassword = "NewPassword123!";
        // Set the new password
        var setPasswordCommand = new SetPasswordCommand
        {
            Email = email,
            Token = token,
            Password = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/set-password", setPasswordCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verify login with old password still works
        var loginCommand = new LoginCommand
        {
            Email = email,
            Password = oldPassword,
        };
        var loginResponse = await Client.PostAsJsonAsync("/user/login", loginCommand);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task CheckEmailAvailability_WithAvailableEmail_ReturnsNoContent()
    {
        // Arrange
        var email = $"available-{Guid.NewGuid()}@example.com";

        // Act
        var response = await Client.GetAsync($"/user/check-email?email={Uri.EscapeDataString(email)}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CheckEmailAvailability_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange: First create a user
        var email = $"existing-{Guid.NewGuid()}@example.com";
        var signUpCommand = new SignUpCommand
        {
            Email = email,
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890"
        };
        var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
        signUpResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync($"/user/check-email?email={Uri.EscapeDataString(email)}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("already in use", responseContent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@invalid.com")]
    public async Task CheckEmailAvailability_WithInvalidEmail_ReturnsBadRequest(string email)
    {
        // Act
        var response = await Client.GetAsync($"/user/check-email?email={Uri.EscapeDataString(email)}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchUsers_WithoutSearchTerm_ReturnsAllUsers()
    {
        // Arrange: Create users
        var users = new[]
        {
            new SignUpCommand
            {
                Email = "test1@example.com",
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!",
                FirstName = "jane",
                LastName = "smith",
                PhoneNumber = "+987654321"
            },
            new SignUpCommand
            {
                Email = "test2@example.com",
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!",
                FirstName = "john",
                LastName = "doe",
                PhoneNumber = "+123456789"
            }
        };

        foreach (var user in users)
        {
            var response = await Client.PostAsJsonAsync("/user/signup", user);
            var raw = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }

        // login as admin
        await Client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        // Act
        var searchResponse = await Client.GetAsync("/user/search?page=1&pageSize=10");

        // Assert
        searchResponse.EnsureSuccessStatusCode();
        var content = await searchResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedList<UserViewModel>>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 2, "Expected at least 2 results");
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Theory]
    [InlineData("john")]
    [InlineData("doe")]
    public async Task SearchUsers_WithSearchTerm_ReturnsMatchingUsers(string searchTerm)
    {
        // Arrange: Create users
        var email = "searchtest-matches-users@example.com";

        // check if email is available - only sign up if it is
        var emailAvailableResponse = await Client.GetAsync($"/user/check-email?email={Uri.EscapeDataString(email)}");
        if (emailAvailableResponse.IsSuccessStatusCode)
        {
            var signUpCommand = new SignUpCommand
            {
                Email = email,
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!",
                FirstName = "john",
                LastName = "doe",
                PhoneNumber = "+1234567890"
            };
            var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
            signUpResponse.EnsureSuccessStatusCode();
        }

        // login as admin
        await Client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        // Act
        var searchResponse = await Client.GetAsync($"/user/search?searchTerm={Uri.EscapeDataString(searchTerm)}&page=1&pageSize=10");

        // Assert
        searchResponse.EnsureSuccessStatusCode();
        var content = await searchResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedList<UserViewModel>>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("john", result.Items[0].FirstName);
        Assert.Equal("doe", result.Items[0].LastName);
    }

    [Fact]
    public async Task SearchUsers_WithPagination_ReturnsCorrectPage()
    {
        // Arrange: Create multiple users
        for (int i = 1; i <= 25; i++)
        {
            var signUpCommand = new SignUpCommand
            {
                Email = $"test-pagination-{i}@example.com",
                Password = "StrongPassword123!",
                ConfirmPassword = "StrongPassword123!",
                FirstName = $"User{i}",
                LastName = "Test",
                PhoneNumber = $"070312345{i}"
            };

            var response = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
            response.EnsureSuccessStatusCode();
        }

        // login as admin
        await Client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        // Act
        var searchResponse = await Client.GetAsync("/user/search?page=2&pageSize=10");

        // Assert
        searchResponse.EnsureSuccessStatusCode();
        var content = await searchResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedList<UserViewModel>>(content, JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 25);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task Lock_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var signUpCommand = new SignUpCommand
        {
            Email = $"lock.test.{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123",
            FirstName = "Lock",
            LastName = "Test",
            PhoneNumber = "+1234567890"
        };
        var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
        Assert.True(signUpResponse.IsSuccessStatusCode, 
            $"Failed to create test user. Status: {signUpResponse.StatusCode}");
        // Get user id from response
        var signedUpUser = JsonSerializer.Deserialize<LoggedInUserModel>(
            await signUpResponse.Content.ReadAsStringAsync(),
            JsonOptions
        );
        Assert.NotNull(signedUpUser);

        // Login as admin
        await Client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var lockCommand = new LockUserCommand
        {
            UserId = signedUpUser.Id!,
            Reason = "Integration test lock",
            LockoutExpiry = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/lock", lockCommand);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify user is locked
        var loginCommand = new LoginCommand
        {
            Email = signUpCommand.Email,
            Password = signUpCommand.Password
        };
        var loginResponse = await Client.PostAsJsonAsync("/user/login", loginCommand);
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Lock_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        // Login as regular user
        await Client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        var lockCommand = new LockUserCommand
        {
            UserId = $"test.{Guid.NewGuid()}@example.com",
            Reason = "Integration test lock",
            LockoutExpiry = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/lock", lockCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Lock_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        // Login as admin
        await Client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var lockCommand = new LockUserCommand
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = "Integration test lock",
            LockoutExpiry = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/lock", lockCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unlock_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var signUpCommand = new SignUpCommand
        {
            Email = $"unlock.test.{Guid.NewGuid()}@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123",
            FirstName = "Unlock",
            LastName = "Test",
            PhoneNumber = "+1234567890"
        };
        var signUpResponse = await Client.PostAsJsonAsync("/user/signup", signUpCommand);
        Assert.True(signUpResponse.IsSuccessStatusCode, 
            $"Failed to create test user. Status: {signUpResponse.StatusCode}");
        
        var signedUpUser = JsonSerializer.Deserialize<LoggedInUserModel>(
            await signUpResponse.Content.ReadAsStringAsync(),
            JsonOptions
        );
        Assert.NotNull(signedUpUser);

        // Login as admin and lock the user
        await Client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var lockCommand = new LockUserCommand
        {
            UserId = signedUpUser.Id!,
            Reason = "Integration test lock",
            LockoutExpiry = DateTime.UtcNow.AddDays(7)
        };
        var lockResponse = await Client.PostAsJsonAsync("/user/lock", lockCommand);
        Assert.Equal(HttpStatusCode.NoContent, lockResponse.StatusCode);

        // Verify user is locked
        var loginCommand = new LoginCommand
        {
            Email = signUpCommand.Email,
            Password = signUpCommand.Password
        };
        var loginResponse = await Client.PostAsJsonAsync("/user/login", loginCommand);
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);

        // Now unlock the user
        var unlockCommand = new UnlockUserCommand
        {
            UserId = signedUpUser.Id!,
            Reason = "Integration test unlock"
        };
        var unlockResponse = await Client.PostAsJsonAsync("/user/unlock", unlockCommand);
        Assert.Equal(HttpStatusCode.NoContent, unlockResponse.StatusCode);

        // Verify user can log in again
        loginResponse = await Client.PostAsJsonAsync("/user/login", loginCommand);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Unlock_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        // Login as regular user
        await Client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        var unlockCommand = new UnlockUserCommand
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = "Integration test unlock"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/unlock", unlockCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Unlock_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        // Login as admin
        await Client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var unlockCommand = new UnlockUserCommand
        {
            UserId = Guid.NewGuid().ToString(),
            Reason = "Integration test unlock"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user/unlock", unlockCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
