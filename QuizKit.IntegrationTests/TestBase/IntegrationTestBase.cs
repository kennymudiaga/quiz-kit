using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuizKit.Common.Enums;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using Xunit;

namespace QuizKit.IntegrationTests.TestBase;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;
    protected readonly CustomWebApplicationFactory Factory;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected async Task<QuizModel> CreateQuizAsync(string title, string description, DateTime? startDate = null, string? categoryId = null, string? organizationId = null)
    {
        var command = new CreateQuizCommand
        {
            Title = title,
            Description = description,
            StartDate = startDate ?? DateTime.UtcNow.AddDays(1),
            CategoryId = categoryId,
            OrganizationId = organizationId
        };

        var response = await PostAsync<QuizModel>("/quiz", command);
        Assert.NotNull(response);
        return response;
    }

    protected async Task UpdateQuizStatusAsync(string quizId, QuizStatus status)
    {
        var command = new UpdateQuizStatusCommand
        {
            Id = quizId,
            Status = status
        };

        await PutAsync<QuizModel>($"/quiz/{quizId}/status", command);
    }

    protected async Task LoginAsAdminAsync()
    {
        await Client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
    }

    protected async Task LoginAsBasicUserAsync()
    {
        await Client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");
    }

    protected void Logout()
    {
        Client.LogOut();
    }

    protected async Task<T?> GetAsync<T>(string url) where T : class
    {
        var response = await Client.GetAsync(url);
        Assert.True(response.IsSuccessStatusCode, $"GET request failed. Status code: {response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    protected async Task<T?> PostAsync<T>(string url, object? content = null) where T : class
    {
        var response = await Client.PostAsJsonAsync(url, content);
        Assert.True(response.IsSuccessStatusCode, $"POST request failed. Status code: {response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    protected async Task<T?> PutAsync<T>(string url, object? content = null) where T : class
    {
        var response = await Client.PutAsJsonAsync(url, content);
        Assert.True(response.IsSuccessStatusCode, $"PUT request failed. Status code: {response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    protected async Task DeleteAsync(string url)
    {
        var response = await Client.DeleteAsync(url);
        Assert.True(response.IsSuccessStatusCode, $"DELETE request failed. Status code: {response.StatusCode}");
    }

    protected async Task<Result<T>> GetResultAsync<T>(string url) where T : class
    {
        var response = await Client.GetAsync(url);
        var content = await response.Content.ReadFromJsonAsync<Result<T>>(JsonOptions);
        Assert.NotNull(content);
        return content;
    }

    protected async Task<Result<T>> PostResultAsync<T>(string url, object? content = null) where T : class
    {
        var response = await Client.PostAsJsonAsync(url, content);
        var result = await response.Content.ReadFromJsonAsync<Result<T>>(JsonOptions);
        Assert.NotNull(result);
        return result;
    }

    protected async Task<Result<T>> PutResultAsync<T>(string url, object? content = null) where T : class
    {
        var response = await Client.PutAsJsonAsync(url, content);
        var result = await response.Content.ReadFromJsonAsync<Result<T>>(JsonOptions);
        Assert.NotNull(result);
        return result;
    }

    protected async Task<Result> DeleteResultAsync(string url)
    {
        var response = await Client.DeleteAsync(url);
        var result = await response.Content.ReadFromJsonAsync<Result>(JsonOptions);
        Assert.NotNull(result);
        return result;
    }
}
