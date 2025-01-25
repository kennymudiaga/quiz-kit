using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Categories;

namespace QuizKit.Tests.RequestHandlers.Categories;

public class GetCategoryQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly GetCategoryQueryHandler _handler;

    public GetCategoryQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        
        var config = new MapperConfiguration(cfg => 
            cfg.CreateMap<Category, CategoryModel>());
        _mapper = config.CreateMapper();
        
        _handler = new GetCategoryQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = "programming",
            Description = "Programming related questions",
            CreationTime = DateTime.UtcNow
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var query = new GetCategoryQuery { Id = "programming" };

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(category.Id, result.Data.Id);
        Assert.Equal(category.Description, result.Data.Description);
        Assert.Equal(category.CreationTime, result.Data.CreationTime);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var query = new GetCategoryQuery { Id = "nonexistent" };

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        Assert.False(result.IsSuccess);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
