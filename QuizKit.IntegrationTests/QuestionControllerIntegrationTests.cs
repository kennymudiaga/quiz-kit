using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using Xunit;

namespace QuizKit.IntegrationTests;

public class QuestionControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly QuizDbContext _context;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public QuestionControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _context = factory.Services.GetRequiredService<QuizDbContext>();
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        // Create a quiz first
        var createQuizCommand = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            TimeLimit = 30,
            RandomizeQuestions = true,
            ShowAnswers = true
        };

        var quizResponse = await _client.PostAsJsonAsync("/quiz", createQuizCommand);
        var quiz = await quizResponse.Content.ReadFromJsonAsync<QuizModel>(_jsonOptions);
        Assert.NotNull(quiz);

        var command = new CreateQuestionCommand
        {
            QuizId = quiz.Id!,
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/quiz/{quiz.Id}/question", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<QuestionModel>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(command.QuestionText, result.QuestionText);
        Assert.Equal(command.A, result.A);
        Assert.Equal(command.B, result.B);
        Assert.Equal(command.C, result.C);
        Assert.Equal(command.D, result.D);
        Assert.Equal(command.Answer, result.Answer);
    }

    [Fact]
    public async Task Create_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new CreateQuestionCommand
        {
            QuizId = "non-existent-quiz",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/non-existent-quiz/question", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange - Login as non-admin user
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        var command = new CreateQuestionCommand
        {
            QuizId = "any-quiz-id",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/any-quiz-id/question", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        // Create a quiz first
        var createQuizCommand = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            TimeLimit = 30,
            RandomizeQuestions = true,
            ShowAnswers = true
        };

        var quizResponse = await _client.PostAsJsonAsync("/quiz", createQuizCommand);
        var quiz = await quizResponse.Content.ReadFromJsonAsync<QuizModel>(_jsonOptions);
        Assert.NotNull(quiz);

        // Create a question
        var createQuestionCommand = new CreateQuestionCommand
        {
            QuizId = quiz.Id!,
            QuestionText = "Original Question",
            A = "Original A",
            B = "Original B",
            C = "Original C",
            D = "Original D",
            Answer = "A"
        };

        var createResponse = await _client.PostAsJsonAsync($"/quiz/{quiz.Id}/question", createQuestionCommand);
        var question = await createResponse.Content.ReadFromJsonAsync<QuestionModel>(_jsonOptions);
        Assert.NotNull(question);

        // Update the question
        var updateCommand = new UpdateQuestionCommand
        {
            Id = question.Id,
            QuizId = quiz.Id!,
            QuestionText = "Updated Question",
            A = "Updated A",
            B = "Updated B",
            C = "Updated C",
            D = "Updated D",
            Answer = "B"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/quiz/{quiz.Id}/question/{question.Id}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<QuestionModel>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(updateCommand.QuestionText, result.QuestionText);
        Assert.Equal(updateCommand.A, result.A);
        Assert.Equal(updateCommand.B, result.B);
        Assert.Equal(updateCommand.C, result.C);
        Assert.Equal(updateCommand.D, result.D);
        Assert.Equal(updateCommand.Answer, result.Answer);
    }

    [Fact]
    public async Task Update_WithNonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new UpdateQuestionCommand
        {
            Id = "999",
            QuizId = "non-existent-quiz",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/quiz/non-existent-quiz/question/999", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange - Login as non-admin user
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        var command = new UpdateQuestionCommand
        {
            Id = "1",
            QuizId = "any-quiz-id",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/quiz/any-quiz-id/question/1", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
