using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuizKit.Common.Models;
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
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            // TODO: Create organization in test setup - then use its ID here
            // OrganizationId = "test-org"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<QuizModel>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(command.Title, result!.Title);
    }

    [Fact]
    public async Task Create_WithNonExistentOrganizationId_ReturnsBadRequest()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            OrganizationId = "non-existent-org"
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
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            OrganizationId = "test-org"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetQuizzes_WithoutFilters_ReturnsAllQuizzes()
    {
        // Arrange
        // login as admin
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
        // Create some test quizzes
        var marker = "Test_Quiz_" + Guid.NewGuid().ToString();
        for (int i = 0; i < 3; i++)
        {
            var command = new CreateQuizCommand
            {
                Title = $"{marker}_{i}",
                Description = $"Test Description {i}",
            };
           var createResponse = await _client.PostAsJsonAsync("/quiz", command);
           Assert.True(createResponse.IsSuccessStatusCode, "Failed to create test quiz");
        }

        // Act
        var response = await _client.GetAsync("/quiz?pageSize=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<QuizModel>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
        // check that all our marked quizzes were matched
        var count = result.Items.Count(x => x.Title!.StartsWith(marker));
        Assert.Equal(3, count);
    }

    [Fact(Skip = "Creating organizations not yet supported")]
    public async Task GetQuizzes_WithOrganizationFilter_ReturnsFilteredQuizzes()
    {
        // Arrange
        const string organizationId = "test-org";

        // Act
        var response = await _client.GetAsync($"/quiz?organizationId={organizationId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<QuizModel>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.All(result!.Items, quiz => Assert.Equal(organizationId, quiz.OrganizationId));
    }

    [Fact]
    public async Task GetQuizzes_WithSearchTerm_ReturnsMatchingQuizzes()
    {
        // Arrange
        // login as admin
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
        // Create some test quizzes
        var marker = "Test_Quiz_" + Guid.NewGuid().ToString();
        for (int i = 0; i < 3; i++)
        {
            var command = new CreateQuizCommand
            {
                Title = $"{marker}_{i}",
                Description = $"Test Description {i}",
            };
            var createResponse = await _client.PostAsJsonAsync("/quiz", command);
            Assert.True(createResponse.IsSuccessStatusCode, "Failed to create test quiz");
        }


        // Act
        var response = await _client.GetAsync($"/quiz?searchTerm={marker}&pageSize=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<QuizModel>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(3, result!.Items.Count);
    }

    [Fact]
    public async Task GetQuizzes_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        // login as admin
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
        // Create some test quizzes
        var marker = "Test_Quiz_" + Guid.NewGuid().ToString();
        for (int i = 0; i < 3; i++)
        {
            var command = new CreateQuizCommand
            {
                Title = $"{marker}_{i}",
                Description = $"Test Description {i}",
            };
            var createResponse = await _client.PostAsJsonAsync("/quiz", command);
            Assert.True(createResponse.IsSuccessStatusCode, "Failed to create test quiz");
        }


        const int page = 2;
        const int pageSize = 1;

        // Act
        var response = await _client.GetAsync($"/quiz?page={page}&pageSize={pageSize}&searchTerm={marker}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<QuizModel>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(page, result!.Page);
        Assert.Equal(pageSize, result.PageSize);
        Assert.NotEmpty(result.Items);
        Assert.Single(result.Items);
        Assert.Equal($"{marker}_1", result.Items.First().Title);
    }

    [Fact]
    public async Task GetQuizzes_WithInvalidPage_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/quiz?page=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        // Create a quiz first
        var createCommand = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            TimeLimit = 30,
            RandomizeQuestions = true,
            ShowAnswers = true
        };

        var createResponse = await _client.PostAsJsonAsync("/quiz", createCommand);
        var createResult = await createResponse.Content.ReadFromJsonAsync<QuizModel>(_jsonOptions);
        Assert.NotNull(createResult);

        // Update the quiz
        var updateCommand = new UpdateQuizCommand
        {
            Id = createResult.Id!,
            Title = "Updated Quiz",
            Description = "Updated Description",
            TimeLimit = 45,
            RandomizeQuestions = false,
            ShowAnswers = false
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/quiz/{createResult.Id}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<QuizModel>(_jsonOptions);
        Assert.NotNull(updateResult);
        Assert.Equal(updateCommand.Title, updateResult.Title);
        Assert.Equal(updateCommand.Description, updateResult.Description);
        Assert.Equal(updateCommand.TimeLimit, updateResult.TimeLimit);
        Assert.Equal(updateCommand.RandomizeQuestions, updateResult.RandomizeQuestions);
        Assert.Equal(updateCommand.ShowAnswers, updateResult.ShowAnswers);
    }

    [Fact]
    public async Task Update_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new UpdateQuizCommand
        {
            Id = "non-existent-id",
            Title = "Updated Quiz",
            Description = "Updated Description"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/quiz/non-existent-id", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        var command = new UpdateQuizCommand
        {
            Id = "some-id",
            Title = "Updated Quiz",
            Description = "Updated Description"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/quiz/some-id", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new UpdateQuizCommand
        {
            Id = "id-in-body",
            Title = "Updated Quiz",
            Description = "Updated Description"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/quiz/different-id", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal("ID in URL must match ID in request body.", result.Message);
    }
}
