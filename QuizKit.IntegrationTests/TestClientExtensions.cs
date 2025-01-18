using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using System.Net.Http.Json;
using System.Text.Json;

namespace QuizKit.IntegrationTests;

public static class TestClientExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    public static async Task LoginAsync(this HttpClient client, string email, string password)
    {
        var loginCommand = new LoginCommand
        {
            Email = email,
            Password = password
        };

        var loginResponse = await client.PostAsJsonAsync("/user/login", loginCommand);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = JsonSerializer.Deserialize<LoggedInUserModel>(
            await loginResponse.Content.ReadAsStringAsync(),
            _jsonOptions
        );

        // Set the authorization token
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent!.Token);
    }

    public static void LogOut(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }
}
