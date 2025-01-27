using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Enums;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Quizzes;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Quizzes;

public class GetPreviewByIdQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetPreviewByIdQueryHandler _handler;

    public GetPreviewByIdQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: $"QuizDb_{Guid.NewGuid()}")
            .Options;

        _context = new QuizDbContext(options);
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Quiz, QuizPreviewModel>();
        });
        _mapper = config.CreateMapper();
        
        _handler = new GetPreviewByIdQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        var query = new GetPreviewByIdQuery { QuizId = "non-existent-id" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Theory]
    [InlineData(QuizStatus.Created)]
    [InlineData(QuizStatus.Closed)]
    public async Task Handle_WithInvalidStatus_ReturnsBadRequest(QuizStatus status)
    {
        // Arrange
        var quiz = TestQuiz.Create(
            id: "test-id",
            title: "Test Quiz",
            description: "Test Description",
            status: status
        );
        quiz.AddQuestion(new QuizQuestion { Id = "q1" });
        quiz.AddQuestion(new QuizQuestion { Id = "q2" });
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var query = new GetPreviewByIdQuery { QuizId = quiz.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal("Quiz not available", result.Message);
    }

    [Theory]
    [InlineData(QuizStatus.Approved)]
    [InlineData(QuizStatus.Live)]
    public async Task Handle_WithValidStatus_ReturnsSuccess(QuizStatus status)
    {
        // Arrange
        var quiz = TestQuiz.Create(
            id: "test-id",
            title: "Test Quiz",
            description: "Test Description",
            status: status
        );
        quiz.AddQuestion(new QuizQuestion { Id = "q1" });
        quiz.AddQuestion(new QuizQuestion { Id = "q2" });
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var query = new GetPreviewByIdQuery { QuizId = quiz.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(quiz.Id, result.Data.Id);
        Assert.Equal(quiz.Title, result.Data.Title);
        Assert.Equal(quiz.Description, result.Data.Description);
        Assert.Equal(2, result.Data.QuestionsCount);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
