using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Categories;
using QuizKit.Core.Data;
using QuizKit.Core.Mappers;
using QuizKit.Core.RequestHandlers.Categories;

namespace QuizKit.Tests.RequestHandlers.Categories;

public class CreateCategoryCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly IMapper _mapper;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        
        var config = new MapperConfiguration(cfg => 
            cfg.AddProfile<CategoryMappingProfile>());
        _mapper = config.CreateMapper();
        
        _handler = new CreateCategoryCommandHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesCategory()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(command.Id, result.Data.Id);
        Assert.Equal(command.Description, result.Data.Description);
        Assert.NotEqual(default, result.Data.CreationTime);

        var category = await _context.Categories.FindAsync(command.Id);
        Assert.NotNull(category);
        Assert.Equal(command.Description, category.Description);
        Assert.NotEqual(default, category.CreationTime);
    }

    [Fact]
    public async Task Handle_WithExistingId_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };
        await _handler.Handle(command, default);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("A category with this ID already exists.", result.Message);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Handle_WithInvalidId_ReturnsBadRequest(string id)
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Id = id,
            Description = "Programming related questions"
        };

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category ID is required.", result.Message);
        Assert.Null(result.Data);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
