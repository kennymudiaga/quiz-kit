using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Quizzes;

namespace QuizKit.Tests.RequestHandlers.Quizzes;

public class GetQuizQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetQuizQueryHandler _handler;

    public GetQuizQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Quiz, QuizModel>();
            cfg.CreateMap<Organization, OrganizationModel>();
            cfg.CreateMap<Category, CategoryModel>();
        });
        _mapper = config.CreateMapper();
        
        _handler = new GetQuizQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsQuiz()
    {
        // Arrange
        var organization = new Organization { Name = "Test Org", Email = "mail@quizkit.com" };
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

        var query = new GetQuizQuery { Id = quiz.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(quiz.Title, result.Data.Title);
        Assert.Equal(quiz.Description, result.Data.Description);
        Assert.Equal(quiz.TimeLimit, result.Data.TimeLimit);
        Assert.Equal(quiz.RandomizeQuestions, result.Data.RandomizeQuestions);
        Assert.Equal(quiz.ShowAnswers, result.Data.ShowAnswers);
        Assert.Equal(quiz.OrganizationId, result.Data.OrganizationId);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var query = new GetQuizQuery { Id = "non-existent-id" };

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
