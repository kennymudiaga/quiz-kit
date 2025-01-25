using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.Mappers;
using QuizKit.Core.RequestHandlers.Questions;

namespace QuizKit.Tests.RequestHandlers.Questions;

public class GetQuestionsQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetQuestionsQueryHandler _handler;

    public GetQuestionsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<QuizMappingProfile>();
        });
        _mapper = config.CreateMapper();
        
        _handler = new GetQuestionsQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidQuizId_ReturnsQuestions()
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

        var questions = new[]
        {
            new QuizQuestion
            {
                QuizId = quiz.Id,
                QuestionText = "Question 1",
                A = "A1",
                B = "B1",
                C = "C1",
                D = "D1",
                Answer = "A"
            },
            new QuizQuestion
            {
                QuizId = quiz.Id,
                QuestionText = "Question 2",
                A = "A2",
                B = "B2",
                C = "C2",
                D = "D2",
                Answer = "B"
            }
        };
        _context.QuizQuestions.AddRange(questions);
        await _context.SaveChangesAsync();

        var query = new GetQuestionsQuery { QuizId = quiz.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(questions[0].QuestionText, result.Data[0].QuestionText);
        Assert.Equal(questions[1].QuestionText, result.Data[1].QuestionText);
    }

    [Fact]
    public async Task Handle_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        var query = new GetQuestionsQuery { QuizId = "non-existent-quiz" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
