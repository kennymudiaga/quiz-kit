using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Models.Users;

namespace QuizKit.Tests.Integration;

public class UserControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
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
        var response = await _client.PostAsJsonAsync("/api/user/signup", signUpCommand);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.True(response.IsSuccessStatusCode, 
            $"Expected success status code, but got {response.StatusCode}. " +
            $"Response content: {responseContent}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = JsonSerializer.Deserialize<LoggedInUserModel>(responseContent, _jsonOptions);
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
        var response = await _client.PostAsJsonAsync("/api/user/signup", signUpCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<Result>(responseContent, _jsonOptions);
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

        var signUpResponse = await _client.PostAsJsonAsync("/api/user/signup", signUpCommand);
        var signUpResponseContent = await signUpResponse.Content.ReadAsStringAsync();
        var signUpContent = JsonSerializer.Deserialize<LoggedInUserModel>(
            signUpResponseContent, 
            _jsonOptions
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

        var loginResponse = await _client.PostAsJsonAsync("/api/user/login", loginCommand);

        // Assert
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        
        Assert.True(loginResponse.IsSuccessStatusCode, 
            $"Expected success status code, but got {loginResponse.StatusCode}. " +
            $"Response content: {loginResponseContent}");
        
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginContent = JsonSerializer.Deserialize<LoggedInUserModel>(loginResponseContent, _jsonOptions);
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

        var signUpResponse = await _client.PostAsJsonAsync("/api/user/signup", signUpCommand);
        var signUpContent = JsonSerializer.Deserialize<LoggedInUserModel>(
            await signUpResponse.Content.ReadAsStringAsync(), 
            _jsonOptions
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

        var loginResponse = await _client.PostAsJsonAsync("/api/user/login", loginCommand);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = JsonSerializer.Deserialize<LoggedInUserModel>(
            await loginResponse.Content.ReadAsStringAsync(), 
            _jsonOptions
        );

        // Set the authorization token
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent!.Token);

        var changePasswordCommand = new ChangePasswordCommand
        {
            CurrentPassword = signUpCommand.Password,
            NewPassword = "NewStrongPassword456!"
        };

        var changePasswordResponse = await _client.PostAsJsonAsync("/api/user/change-password", changePasswordCommand);

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

        var newLoginResponse = await _client.PostAsJsonAsync("/api/user/login", newLoginCommand);
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
        var response = await _client.PostAsJsonAsync("/api/user/login", loginCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<Result>(responseContent, _jsonOptions);
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
        var response = await _client.PostAsJsonAsync("/api/user/change-password", changePasswordCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
