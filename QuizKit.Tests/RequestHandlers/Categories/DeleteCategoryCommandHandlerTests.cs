using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Categories;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.RequestHandlers.Categories;

namespace QuizKit.Tests.RequestHandlers.Categories;

public class DeleteCategoryCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        _handler = new DeleteCategoryCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidCommand_DeletesCategory()
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

        var command = new DeleteCategoryCommand { Id = "programming" };

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        var deletedCategory = await _context.Categories.FindAsync(command.Id);
        Assert.Null(deletedCategory);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var command = new DeleteCategoryCommand { Id = "nonexistent" };

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_WithCategoryInUse_ReturnsBadRequest()
    {
        // Arrange
        var category = new Category
        {
            Id = "programming",
            Description = "Programming related questions",
            CreationTime = DateTime.UtcNow
        };
        _context.Categories.Add(category);

        var quiz = new Quiz(new CreateQuizCommand
        {
            Title = "Programming Quiz",
            Description = "Programming Quiz Description",
            CategoryId = category.Id,
        });
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var command = new DeleteCategoryCommand { Id = "programming" };

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete category as it is being used by one or more quizzes.", result.Message);
        var categoryStillExists = await _context.Categories.FindAsync(command.Id);
        Assert.NotNull(categoryStillExists);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
