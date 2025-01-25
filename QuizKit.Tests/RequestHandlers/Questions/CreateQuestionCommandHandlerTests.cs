using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Questions;

namespace QuizKit.Tests.RequestHandlers.Questions;

public class CreateQuestionCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly CreateQuestionCommandHandler _handler;

    public CreateQuestionCommandHandlerTests()
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
        
        _handler = new CreateQuestionCommandHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesQuestion()
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
        await _context.SaveChangesAsync();

        var command = new CreateQuestionCommand
        {
            QuizId = quiz.Id,
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
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(command.QuestionText, result.Data.QuestionText);
        Assert.Equal(command.A, result.Data.A);
        Assert.Equal(command.B, result.Data.B);
        Assert.Equal(command.C, result.Data.C);
        Assert.Equal(command.D, result.Data.D);
        Assert.Equal(command.Answer, result.Data.Answer);

        var savedQuestion = await _context.QuizQuestions.FirstOrDefaultAsync(q => q.QuizId == quiz.Id);
        Assert.NotNull(savedQuestion);
        Assert.Equal(command.QuestionText, savedQuestion.QuestionText);
    }

    [Fact]
    public async Task Handle_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
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
