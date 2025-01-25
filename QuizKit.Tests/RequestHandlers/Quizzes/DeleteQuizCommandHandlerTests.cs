using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Quizzes;

namespace QuizKit.Tests.RequestHandlers.Quizzes;

public class DeleteQuizCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly DeleteQuizCommandHandler _handler;

    public DeleteQuizCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        _handler = new DeleteQuizCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidCommand_DeletesQuiz()
    {
        // Arrange
        var quiz = new Quiz(new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description"
        });
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var command = new DeleteQuizCommand { Id = quiz.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var deletedQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quiz.Id);
        Assert.Null(deletedQuiz);
    }

    [Fact]
    public async Task Handle_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        var command = new DeleteQuizCommand { Id = "non-existent-id" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Quiz not found.", result.Message);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
