using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Quizzes;

namespace QuizKit.Tests.RequestHandlers.Quizzes;

public class UpdateQuizCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly UpdateQuizCommandHandler _handler;

    public UpdateQuizCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        _handler = new UpdateQuizCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesQuiz()
    {
        // Arrange
        var organization = new Organization { Name = "Test Org", Email = "mail@quizkit.com" };
        _context.Organizations.Add(organization);

        var quiz = new Quiz(new CreateQuizCommand
        {
            Title = "Original Title",
            Description = "Original Description",
            TimeLimit = 30,
            RandomizeQuestions = true,
            ShowAnswers = true,
            OrganizationId = organization.Id
        });
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var command = new UpdateQuizCommand
        {
            Id = quiz.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            TimeLimit = 45,
            RandomizeQuestions = false,
            ShowAnswers = false,
            OrganizationId = organization.Id
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(command.Title, result.Data.Title);
        Assert.Equal(command.Description, result.Data.Description);
        Assert.Equal(command.TimeLimit, result.Data.TimeLimit);
        Assert.Equal(command.RandomizeQuestions, result.Data.RandomizeQuestions);
        Assert.Equal(command.ShowAnswers, result.Data.ShowAnswers);
    }

    [Fact]
    public async Task Handle_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        var command = new UpdateQuizCommand
        {
            Id = "non-existent-id",
            Title = "Updated Title",
            Description = "Updated Description"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrganization_ReturnsBadRequest()
    {
        // Arrange
        var quiz = new Quiz(new CreateQuizCommand
        {
            Title = "Original Title",
            Description = "Original Description"
        });
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var command = new UpdateQuizCommand
        {
            Id = quiz.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            OrganizationId = "non-existent-org"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Organization not found.", result.Message);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ReturnsBadRequest()
    {
        // Arrange
        var quiz = new Quiz(new CreateQuizCommand
        {
            Title = "Original Title",
            Description = "Original Description"
        });
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var command = new UpdateQuizCommand
        {
            Id = quiz.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            CategoryId = "non-existent-category"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Message);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
