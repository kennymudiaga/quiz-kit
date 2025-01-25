using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Questions;

namespace QuizKit.Tests.RequestHandlers.Questions;

public class UpdateQuestionCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly UpdateQuestionCommandHandler _handler;

    public UpdateQuestionCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<QuizQuestion, QuestionModel>();
        });
        _mapper = config.CreateMapper();
        
        _handler = new UpdateQuestionCommandHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesQuestion()
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
            QuestionText = "Original Question",
            A = "Original A",
            B = "Original B",
            C = "Original C",
            D = "Original D",
            Answer = "A"
        };
        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync();

        var command = new UpdateQuestionCommand
        {
            Id = question.Id,
            QuizId = quiz.Id,
            QuestionText = "Updated Question",
            A = "Updated A",
            B = "Updated B",
            C = "Updated C",
            D = "Updated D",
            Answer = "B"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(command.QuestionText, result.Data.QuestionText);
        Assert.Equal(command.A, result.Data.A);
        Assert.Equal(command.B, result.Data.B);
        Assert.Equal(command.C, result.Data.C);
        Assert.Equal(command.D, result.Data.D);
        Assert.Equal(command.Answer, result.Data.Answer);

        var updatedQuestion = await _context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == question.Id);
        Assert.NotNull(updatedQuestion);
        Assert.Equal(command.QuestionText, updatedQuestion.QuestionText);
        Assert.Equal(command.Answer, updatedQuestion.Answer);
    }

    [Fact]
    public async Task Handle_WithNonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
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
