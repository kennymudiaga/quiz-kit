using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Enums;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Mappers;
using QuizKit.Core.RequestHandlers.Quizzes;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Quizzes;

public class GetPreviewsQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetPreviewsQueryHandler _handler;

    public GetPreviewsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        var config = new MapperConfiguration(cfg => cfg.AddProfile<QuizMappingProfile>());
        _mapper = config.CreateMapper();
        _handler = new GetPreviewsQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsOnlyApprovedAndLiveQuizzes()
    {
        // Arrange
        var quiz1 = TestQuiz.Create(
            id: "quiz-1",
            title: "Quiz 1",
            categoryId: "cat-1",
            organizationId: "org-1",
            createdAt: DateTime.UtcNow.AddDays(-1),
            status: QuizStatus.Approved);

        var quiz2 = TestQuiz.Create(
            id: "quiz-2",
            title: "Quiz 2",
            categoryId: "cat-1",
            organizationId: "org-1",
            createdAt: DateTime.UtcNow,
            status: QuizStatus.Live);

        var quiz3 = TestQuiz.Create(
            id: "quiz-3",
            title: "Quiz 3",
            categoryId: "cat-1",
            organizationId: "org-1",
            createdAt: DateTime.UtcNow,
            status: QuizStatus.Created);

        await _context.Quizzes.AddRangeAsync(quiz1, quiz2, quiz3);
        await _context.SaveChangesAsync();

        var query = new GetPreviewsQuery
        {
            OrganizationId = "org-1",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Equal("Quiz 2", result.Data.Items[0].Title); // Most recent first
        Assert.Equal("Quiz 1", result.Data.Items[1].Title);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ReturnsMatchingQuizzes()
    {
        // Arrange
        var quiz1 = TestQuiz.Create(
            id: "quiz-1",
            title: "Math Quiz",
            status: QuizStatus.Approved);

        var quiz2 = TestQuiz.Create(
            id: "quiz-2",
            title: "Science Quiz",
            status: QuizStatus.Live);

        var quiz3 = TestQuiz.Create(
            id: "quiz-3",
            title: "Hidden Math Quiz",
            status: QuizStatus.Created);

        await _context.Quizzes.AddRangeAsync(quiz1, quiz2, quiz3);
        await _context.SaveChangesAsync();

        var query = new GetPreviewsQuery
        {
            SearchTerm = "math",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
        Assert.Single(result.Data.Items);
        Assert.Equal("Math Quiz", result.Data.Items[0].Title);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
