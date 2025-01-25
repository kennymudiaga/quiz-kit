using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.Mappers;
using QuizKit.Core.RequestHandlers.Questions;

namespace QuizKit.Tests.RequestHandlers.Questions;

public class GetQuestionQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetQuestionQueryHandler _handler;

    public GetQuestionQueryHandlerTests()
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
        
        _handler = new GetQuestionQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsQuestion()
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

        var query = new GetQuestionQuery { QuizId = quiz.Id, Id = question.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(question.QuestionText, result.Data.QuestionText);
        Assert.Equal(question.A, result.Data.A);
        Assert.Equal(question.B, result.Data.B);
        Assert.Equal(question.C, result.Data.C);
        Assert.Equal(question.D, result.Data.D);
        Assert.Equal(question.Answer, result.Data.Answer);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var query = new GetQuestionQuery { QuizId = "non-existent-quiz", Id = "non-existent-id" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
