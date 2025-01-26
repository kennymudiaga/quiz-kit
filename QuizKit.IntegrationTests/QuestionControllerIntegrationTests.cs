using System.Net;
using System.Net.Http.Json;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.IntegrationTests.TestBase;
using Xunit;

namespace QuizKit.IntegrationTests;

public class QuestionControllerIntegrationTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

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
        var result = await PostAsync<QuestionModel>($"/quiz/{quiz.Id}/question", command);

        // Assert
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
        await LoginAsAdminAsync();

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
        var response = await Client.PostAsJsonAsync($"/quiz/non-existent-quiz/question", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        // Login as Admin
        await LoginAsAdminAsync();

        // Create a quiz
        var quiz = await CreateQuizAsync($"Test Quiz {Guid.NewGuid()}", "Test Description");

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
        // Arrange - Login as non-admin user
        await LoginAsBasicUserAsync();
        // Act
        var response = await Client.PostAsJsonAsync($"/quiz/{quiz.Id}/question", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        var createCommand = new CreateQuestionCommand
        {
            QuizId = quiz.Id!,
            QuestionText = "Original Question",
            A = "Original A",
            B = "Original B",
            C = "Original C",
            D = "Original D",
            Answer = "A"
        };

        var question = await PostAsync<QuestionModel>($"/quiz/{quiz.Id}/question", createCommand);
        Assert.NotNull(question);
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
        var result = await PutAsync<QuestionModel>($"/quiz/{quiz.Id}/question/{question.Id}", updateCommand);

        // Assert
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
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        var command = new UpdateQuestionCommand
        {
            Id = "999",
            QuizId = quiz.Id!,
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/quiz/{quiz.Id}/question/999", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        // Login as admin user
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        var createCommand = new CreateQuestionCommand
        {
            QuizId = quiz.Id!,
            QuestionText = "Original Question",
            A = "Original A",
            B = "Original B",
            C = "Original C",
            D = "Original D",
            Answer = "A"
        };

        var question = await PostAsync<QuestionModel>($"/quiz/{quiz.Id}/question", createCommand);
        Assert.NotNull(question);

        // Login as non-admin user
        await LoginAsBasicUserAsync();
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
        var response = await Client.PutAsJsonAsync($"/quiz/{quiz.Id}/question/{question.Id}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetQuestions_WithValidQuizId_ReturnsSuccess()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        var questions = new[]
        {
            new CreateQuestionCommand
            {
                QuizId = quiz.Id!,
                QuestionText = "Question 1",
                A = "A1",
                B = "B1",
                C = "C1",
                D = "D1",
                Answer = "A"
            },
            new CreateQuestionCommand
            {
                QuizId = quiz.Id!,
                QuestionText = "Question 2",
                A = "A2",
                B = "B2",
                C = "C2",
                D = "D2",
                Answer = "B"
            }
        };

        foreach (var question in questions)
        {
            await PostAsync<QuestionModel>($"/quiz/{quiz.Id}/question", question);
        }

        // Act
        var result = await GetAsync<List<QuestionModel>>($"/quiz/{quiz.Id}/question");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, q => q.QuestionText == "Question 1");
        Assert.Contains(result, q => q.QuestionText == "Question 2");
    }

    [Fact]
    public async Task GetQuestions_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/quiz/non-existent-quiz/question");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidIds_ReturnsSuccess()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        var createCommand = new CreateQuestionCommand
        {
            QuizId = quiz.Id!,
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        var question = await PostAsync<QuestionModel>($"/quiz/{quiz.Id}/question", createCommand);

        // Act
        var result = await GetAsync<QuestionModel>($"/quiz/{quiz.Id}/question/{question.Id}");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createCommand.QuestionText, result.QuestionText);
        Assert.Equal(createCommand.A, result.A);
        Assert.Equal(createCommand.B, result.B);
        Assert.Equal(createCommand.C, result.C);
        Assert.Equal(createCommand.D, result.D);
        Assert.Equal(createCommand.Answer, result.Answer);
    }

    [Fact]
    public async Task Get_WithNonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.GetAsync("/quiz/non-existent-quiz/question/non-existent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithValidIds_ReturnsNoContent()
    {
        // Arrange
        await LoginAsAdminAsync();

        var quiz = await CreateQuizAsync("Test Quiz", "Test Description");

        var createCommand = new CreateQuestionCommand
        {
            QuizId = quiz.Id!,
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        var question = await PostAsync<QuestionModel>($"/quiz/{quiz.Id}/question", createCommand);

        // Act
        await DeleteAsync($"/quiz/{quiz.Id}/question/{question.Id}");

        // Verify question is deleted
        var getResponse = await Client.GetAsync($"/quiz/{quiz.Id}/question/{question.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Act
        var response = await Client.DeleteAsync("/quiz/non-existent-quiz/question/non-existent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
