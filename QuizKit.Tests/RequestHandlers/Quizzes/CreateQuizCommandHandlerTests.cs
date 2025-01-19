using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Quizzes;

namespace QuizKit.Tests.RequestHandlers.Quizzes;

public class CreateQuizCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly CreateQuizCommandHandler _handler;

    public CreateQuizCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        _handler = new CreateQuizCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesQuiz()
    {
        // Arrange
        var organization = new Organization { Name = "Test Org" };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            TimeLimit = 30,
            RandomizeQuestions = true,
            ShowAnswers = true,
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
        Assert.Equal(command.OrganizationId, result.Data.OrganizationId);
        Assert.NotNull(result.Data.CreatedAt);
        Assert.NotNull(result.Data.Questions);
        Assert.Empty(result.Data.Questions);

        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == result.Data.Id);
        Assert.NotNull(quiz);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrganization_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            OrganizationId = "nonexistent"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Organization not found.", result.Message);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
