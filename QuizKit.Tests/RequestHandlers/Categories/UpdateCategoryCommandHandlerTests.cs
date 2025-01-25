using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Categories;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.Mappers;
using QuizKit.Core.RequestHandlers.Categories;

namespace QuizKit.Tests.RequestHandlers.Categories;

public class UpdateCategoryCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        var mapperConfig = new MapperConfiguration(options => {
            options.AddProfile<CategoryMappingProfile>();
        });
        _handler = new UpdateCategoryCommandHandler(_context, mapperConfig.CreateMapper());
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = "programming",
            Description = "Original description",
            CreationTime = DateTime.UtcNow
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var command = new UpdateCategoryCommand
        {
            Id = "programming",
            Description = "Updated description"
        };

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedCategory = await _context.Categories.FindAsync(command.Id);
        Assert.NotNull(updatedCategory);
        Assert.Equal(command.Description, updatedCategory.Description);
        Assert.NotNull(updatedCategory.LastUpdateTime);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "nonexistent",
            Description = "Updated description"
        };

        // Act
        var result = await _handler.Handle(command, default);

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
