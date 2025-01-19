using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using Xunit;

namespace QuizKit.IntegrationTests;

public class QuizControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public QuizControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        // Login as admin
        await _client.LoginAsync(LoginTestUserBehavior.adminUserEmail, "StrongPassword123!");

        var command = new CreateQuizCommand
        {
            Title = $"Test Quiz {Guid.NewGuid()}",
            Description = "Test Description",
            TimeLimit = 30,
            RandomizeQuestions = true,
            ShowAnswers = true,
            // TODO: Create organization first
            //OrganizationId = "test-org"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var quiz = JsonSerializer.Deserialize<QuizModel>(
            await response.Content.ReadAsStringAsync(),
            _jsonOptions
        );
        Assert.NotNull(quiz);
        Assert.Equal(command.Title, quiz.Title);
        Assert.Equal(command.Description, quiz.Description);
        Assert.Equal(command.TimeLimit, quiz.TimeLimit);
        Assert.Equal(command.RandomizeQuestions, quiz.RandomizeQuestions);
        Assert.Equal(command.ShowAnswers, quiz.ShowAnswers);
        Assert.Equal(command.OrganizationId, quiz.OrganizationId);
        Assert.NotNull(quiz.CreatedAt);
        Assert.NotNull(quiz.Questions);
        Assert.Empty(quiz.Questions);
    }

    
    [Fact]
    public async Task Create_WithNonExistentOrganizationId_ReturnsBadRequest()
    {
        // Arrange
        // Login as admin
        await _client.LoginAsync(LoginTestUserBehavior.adminUserEmail, "StrongPassword123!");

        var command = new CreateQuizCommand
        {
            Title = $"Test Quiz {Guid.NewGuid()}",
            Description = "Test Description",
            OrganizationId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = JsonSerializer.Deserialize<Result>(await response.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(error);
        Assert.Equal("Organization not found.", error.Message);
    }
    

    [Fact]
    public async Task Create_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        // Login as basic user
        await _client.LoginAsync(LoginTestUserBehavior.basicUserEmail, "StrongPassword123!");

        var command = new CreateQuizCommand
        {
            Title = $"Test Quiz {Guid.NewGuid()}",
            Description = "Test Description",
            OrganizationId = "test-org"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }   
}
