using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Quizzes;
using QuizKit.Tests.TestHelpers;
using Xunit;

namespace QuizKit.Tests.RequestHandlers.Quizzes;

public class GetQuizzesQueryHandlerTests
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetQuizzesQueryHandler _handler;

    public GetQuizzesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: $"QuizDb_{Guid.NewGuid()}")
            .Options;

        _context = new QuizDbContext(options);
        _mapper = CreateMapper();
        _handler = new GetQuizzesQueryHandler(_context, _mapper);

        SeedDatabase();
    }

    [Fact]
    public async Task Handle_WithDefaultParameters_ReturnsCorrectPageSize()
    {
        // Arrange
        var query = new GetQuizzesQuery { Page = 1, PageSize = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Equal(5, result.Data.TotalCount);
        Assert.Equal(1, result.Data.Page);
        Assert.Equal(2, result.Data.PageSize);
    }

    [Fact]
    public async Task Handle_WithOrganizationFilter_ReturnsCorrectQuizzes()
    {
        // Arrange
        var organizationId = "org1";
        var query = new GetQuizzesQuery { OrganizationId = organizationId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.All(result.Data.Items, quiz => Assert.Equal(organizationId, quiz.OrganizationId));
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ReturnsMatchingQuizzes()
    {
        // Arrange
        var searchTerm = "quiz 1";
        var query = new GetQuizzesQuery { SearchTerm = searchTerm };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Data.Items, quiz => quiz.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ReturnsCorrectQuizzes()
    {
        // Arrange
        var categoryId = "cat1";
        var query = new GetQuizzesQuery { CategoryId = categoryId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.All(result.Data.Items, quiz => Assert.Equal(categoryId, quiz.CategoryId));
    }

    private void SeedDatabase()
    {
        var organizations = new List<Organization>
        {
            new() { Id = "org1", Name = "Organization 1" },
            new() { Id = "org2", Name = "Organization 2" }
        };

        var categories = new List<Category>
        {
            new() { Id = "cat1" },
            new() { Id = "cat2" }
        };

        var quizzes = new List<Quiz>
        {
            TestQuiz.Create("1", "Quiz 1", "org1", "cat1", DateTime.UtcNow.AddDays(-1)),
            TestQuiz.Create("2", "Quiz 2", "org1", "cat1", DateTime.UtcNow.AddDays(-2)),
            TestQuiz.Create("3", "Quiz 3", "org2", "cat2", DateTime.UtcNow.AddDays(-3)),
            TestQuiz.Create("4", "Quiz 4", "org2", "cat2", DateTime.UtcNow.AddDays(-4)),
            TestQuiz.Create("5", "Quiz 5", "org1", "cat1", DateTime.UtcNow.AddDays(-5))
        };

        _context.Organizations.AddRange(organizations);
        _context.Categories.AddRange(categories);
        _context.Quizzes.AddRange(quizzes);
        _context.SaveChanges();
    }

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Quiz, QuizModel>();
            cfg.CreateMap<Category, CategoryModel>();
            cfg.CreateMap<Organization, OrganizationModel>();
        });

        return configuration.CreateMapper();
    }
}
