using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Categories;

namespace QuizKit.Tests.RequestHandlers.Categories;

public class GetCategoriesQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        
        var config = new MapperConfiguration(cfg => 
            cfg.CreateMap<Category, CategoryModel>());
        _mapper = config.CreateMapper();
        
        _handler = new GetCategoriesQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllCategories()
    {
        // Arrange
        var categories = new[]
        {
            new Category { Id = "programming", Description = "Programming", CreationTime = DateTime.UtcNow },
            new Category { Id = "history", Description = "History", CreationTime = DateTime.UtcNow }
        };
        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();

        var query = new GetCategoriesQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
        Assert.Equal(2, result.Data.Items.Count);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ReturnsMatchingCategories()
    {
        // Arrange
        var categories = new[]
        {
            new Category { Id = "programming", Description = "Programming", CreationTime = DateTime.UtcNow },
            new Category { Id = "history", Description = "History", CreationTime = DateTime.UtcNow }
        };
        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();

        var query = new GetCategoriesQuery 
        { 
            SearchTerm = "prog",
            Page = 1, 
            PageSize = 10 
        };

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
        Assert.Single(result.Data.Items);
        Assert.Equal("programming", result.Data.Items[0].Id);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var categories = new[]
        {
            new Category { Id = "programming", Description = "Programming", CreationTime = DateTime.UtcNow },
            new Category { Id = "history", Description = "History", CreationTime = DateTime.UtcNow },
            new Category { Id = "science", Description = "Science", CreationTime = DateTime.UtcNow }
        };
        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();

        var query = new GetCategoriesQuery 
        { 
            Page = 2, 
            PageSize = 2 
        };

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.TotalCount);
        Assert.Single(result.Data.Items);
        Assert.Equal(2, result.Data.Page);
        Assert.Equal(2, result.Data.PageSize);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
