using System.Net;
using System.Net.Http.Json;
using QuizKit.Common.Enums;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Utils;
using QuizKit.IntegrationTests.TestBase;
using Xunit;

namespace QuizKit.IntegrationTests;

public class QuizControllerIntegrationTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await LoginAsAdminAsync();

        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await PostAsync<QuizModel>("/quiz", command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Description, result.Description);
    }

    [Fact]
    public async Task Create_WithNonExistentOrganizationId_ReturnsBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            OrganizationId = "non-existent-org",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/quiz", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        await LoginAsBasicUserAsync();

        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/quiz", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetQuizzes_WithoutFilters_ReturnsAllQuizzes()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create two quizzes
        await CreateQuizAsync("Quiz 1", "Description 1");
        await CreateQuizAsync("Quiz 2", "Description 2");

        // Act
        var result = await GetAsync<PagedList<QuizModel>>("/quiz");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count >= 2);
        Assert.Contains(result.Items, q => q.Title == "Quiz 1");
        Assert.Contains(result.Items, q => q.Title == "Quiz 2");
    }

    [Fact(Skip = "Organizations not yet supported.")]
    public async Task GetQuizzes_WithOrganizationFilter_ReturnsFilteredQuizzes()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create quizzes with different organizations
        await CreateQuizAsync("Quiz 1", "Description 1", organizationId: "org-1");
        await CreateQuizAsync("Quiz 2", "Description 2", organizationId: "org-2");

        // Act
        var result = await GetAsync<PagedList<QuizModel>>("/quiz?organizationId=org-1");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Quiz 1", result.Items[0].Title);
    }

    [Fact]
    public async Task GetQuizzes_WithSearchTerm_ReturnsMatchingQuizzes()
    {
        // Arrange
        await LoginAsAdminAsync();

        var marker = IdGenerator.GenerateId();
        var title = $"Math {marker} Quiz";
        // Create quizzes with different titles
        await CreateQuizAsync(title, "Math Description");
        await CreateQuizAsync("Science Quiz", "Science Description");

        // Act
        var result = await GetAsync<PagedList<QuizModel>>($"/quiz?searchTerm={title}");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(title, result.Items[0].Title);
    }

    [Fact]
    public async Task GetQuizzes_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create multiple quizzes
        for (int i = 1; i <= 5; i++)
        {
            await CreateQuizAsync($"Quiz {i}", $"Description {i}");
        }

        // Act - Get second page with 2 items per page
        var result = await GetAsync<PagedList<QuizModel>>("/quiz?page=2&pageSize=2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.True(result.TotalCount >= 5);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetQuizzes_WithInvalidPage_ReturnsBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/quiz?page=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQuizzes_WithStatusFilter_ReturnsFilteredQuizzes()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create and update quiz statuses
        var quiz1 = await CreateQuizAsync("Quiz 1", "Description 1");
        await UpdateQuizStatusAsync(quiz1.Id!, QuizStatus.Live);

        var quiz2 = await CreateQuizAsync("Quiz 2", "Description 2");
        await UpdateQuizStatusAsync(quiz2.Id!, QuizStatus.Approved);

        // Act
        var result = await GetAsync<PagedList<QuizModel>>("/quiz?status=Live");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Quiz 1", result.Items[0].Title);
        Assert.Equal(QuizStatus.Live, result.Items[0].Status);
    }

    [Fact]
    public async Task GetQuizzes_WithStatusAndSearchTerm_ReturnsCombinedFilteredQuizzes()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create and update quiz statuses
        var quiz1 = await CreateQuizAsync("Math Quiz", "Math Description");
        await UpdateQuizStatusAsync(quiz1.Id!, QuizStatus.Live);

        var quiz2 = await CreateQuizAsync("Science Quiz", "Science Description");
        await UpdateQuizStatusAsync(quiz2.Id!, QuizStatus.Live);

        // Act
        var result = await GetAsync<PagedList<QuizModel>>("/quiz?status=Live&searchTerm=math");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Math Quiz", result.Items[0].Title);
        Assert.Equal(QuizStatus.Live, result.Items[0].Status);
    }

    [Fact]
    public async Task Update_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create a quiz to update
        var quiz = await CreateQuizAsync("Original Title", "Original Description");

        var command = new UpdateQuizCommand
        {
            Id = quiz.Id!,
            Title = "Updated Title",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await PutAsync<QuizModel>($"/quiz/{quiz.Id}", command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Description, result.Description);
    }

    [Fact]
    public async Task Update_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        var command = new UpdateQuizCommand
        {
            Id = "non-existent-id",
            Title = "Updated Title",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var response = await Client.PutAsJsonAsync("/quiz/non-existent-id", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        await LoginAsAdminAsync();
        var quiz = await CreateQuizAsync("Original Title", "Original Description");
        
        await LoginAsBasicUserAsync();

        var command = new UpdateQuizCommand
        {
            Id = quiz.Id!,
            Title = "Updated Title",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/quiz/{quiz.Id}", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Original Title", "Original Description");

        var command = new UpdateQuizCommand
        {
            Id = "different-id",
            Title = "Updated Title",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/quiz/{quiz.Id}", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("ID in URL must match ID in request body.", result.Message);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsSuccess()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        // Act
        await DeleteAsync($"/quiz/{quiz.Id}");

        // Verify
        var response = await Client.GetAsync($"/quiz/{quiz.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.DeleteAsync("/quiz/non-existent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        await LoginAsAdminAsync();
        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        await LoginAsBasicUserAsync();

        // Act
        var response = await Client.DeleteAsync($"/quiz/{quiz.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidId_ReturnsQuiz()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        // Act
        var result = await GetAsync<QuizModel>($"/quiz/{quiz.Id}");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(quiz.Id, result.Id);
        Assert.Equal(quiz.Title, result.Title);
        Assert.Equal(quiz.Description, result.Description);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/quiz/non-existent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithoutAdminUser_ReturnsForbidden()
    {
        // Arrange
        await LoginAsAdminAsync();
        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        await LoginAsBasicUserAsync();

        var command = new UpdateQuizStatusCommand
        {
            Id = quiz.Id!,
            Status = QuizStatus.Live
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/quiz/{quiz.Id}/status", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithValidArguments_ReturnsOk()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        var command = new UpdateQuizStatusCommand
        {
            Id = quiz.Id!,
            Status = QuizStatus.Live
        };

        // Act
        var result = await PutAsync<QuizModel>($"/quiz/{quiz.Id}/status", command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(QuizStatus.Live, result.Status);
    }

    [Fact]
    public async Task UpdateStatus_ClosedQuiz_ReturnsBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");
        await UpdateQuizStatusAsync(quiz.Id!, QuizStatus.Closed);

        var command = new UpdateQuizStatusCommand
        {
            Id = quiz.Id!,
            Status = QuizStatus.Live
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/quiz/{quiz.Id}/status", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_5MinutesFromeTime_ReturnsBadRequest()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description", DateTime.UtcNow.AddMinutes(5));
        //await UpdateQuizStatusAsync(quiz.Id!, QuizStatus.Live);

        var command = new UpdateQuizStatusCommand
        {
            Id = quiz.Id!,
            Status = QuizStatus.Closed
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/quiz/{quiz.Id}/status", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
