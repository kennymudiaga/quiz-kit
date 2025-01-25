using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Questions;

namespace QuizKit.Tests.RequestHandlers.Questions;

public class DeleteQuestionCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly DeleteQuestionCommandHandler _handler;

    public DeleteQuestionCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        _handler = new DeleteQuestionCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidId_DeletesQuestion()
    {
        // Arrange
        var organization = new Organization { Name = "Test Org", Email = "test@example.com" };
        _context.Organizations.Add(organization);

        var quiz = new Quiz(new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            TimeLimit = 30,
            RandomizeQuestions = true,
            ShowAnswers = true,
            OrganizationId = organization.Id
        });
        _context.Quizzes.Add(quiz);

        var question = new QuizQuestion
        {
            QuizId = quiz.Id,
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };
        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync();

        var command = new DeleteQuestionCommand { QuizId = quiz.Id, Id = question.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var deletedQuestion = await _context.QuizQuestions.FindAsync(question.Id);
        Assert.Null(deletedQuestion);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var command = new DeleteQuestionCommand { QuizId = "non-existent-quiz", Id = "non-existent-id" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
