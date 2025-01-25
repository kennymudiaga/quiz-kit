using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Controllers;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;

namespace QuizKit.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new CategoryController(_mockMediator.Object);
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsCreated()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Created(new CategoryModel
            {
                CreationTime = DateTime.UtcNow,
                Description = command.Description,
                Id = command.Id,
                LastUpdateTime = DateTime.UtcNow
            }));

        // Act
        var result = await _controller.Create(command, default);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        var category = Assert.IsType<CategoryModel>(createdResult.Value);
        Assert.Equal(command.Id, category.Id);
        Assert.Equal(command.Description, category.Description);
    }

    [Fact]
    public async Task Create_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateCategoryCommand()
        {
            Id = null!,
            Description = null!
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.BadRequest("Invalid category details"));

        // Act
        var result = await _controller.Create(command, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid category details", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOk()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "programming",
            Description = "Updated description"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new CategoryModel { Id = command.Id, Description = command.Description }));

        // Act
        var result = await _controller.Update("programming", command, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        var category = Assert.IsType<CategoryModel>(okResult.Value);
        Assert.Equal(command.Id, category.Id);
        Assert.Equal(command.Description, category.Description);
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var command = new UpdateCategoryCommand
        {
            Id = "programming",
            Description = "Updated description"
        };

        // Act
        var result = await _controller.Update("different-id", command, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Category ID mismatch", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Delete("programming", default);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.BadRequest("Category not found"));

        // Act
        var result = await _controller.Delete("nonexistent", default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Category not found", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task Get_WithValidId_ReturnsCategory()
    {
        // Arrange
        var category = new CategoryModel
        {
            Id = "programming",
            Description = "Programming related questions",
            CreationTime = DateTime.UtcNow,
            LastUpdateTime = null
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetCategoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(category));

        // Act
        var result = await _controller.Get("programming", default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<CategoryModel>(okResult.Value);
        Assert.Equal("programming", returnedCategory.Id);
    }

    [Fact]
    public async Task Get_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetCategoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.BadRequest("Category not found"));

        // Act
        var result = await _controller.Get("nonexistent", default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Category not found", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task GetAll_WithValidParameters_ReturnsCategories()
    {
        // Arrange
        var categories = new List<CategoryModel>
        {
            new() { Id = "programming", Description = "Programming related questions", CreationTime = DateTime.UtcNow },
            new() { Id = "history", Description = "History related questions", CreationTime = DateTime.UtcNow }
        };
        var pagedList = new PagedList<CategoryModel>
        {
            Items = categories,
            Page = 1,
            PageSize = 20,
            TotalCount = 2
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetCategoriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedList));

        // Act
        var result = await _controller.GetAll(null, 1, 20, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<PagedList<CategoryModel>>(okResult.Value);
        Assert.Equal(2, data.Items.Count);
        Assert.Equal(2, data.TotalCount);
    }

    [Theory]
    [InlineData(0, 20)]  // Invalid page
    [InlineData(1, 0)]   // Invalid page size
    [InlineData(1, 101)] // Page size too large
    public async Task GetAll_WithInvalidParameters_ReturnsBadRequest(int page, int pageSize)
    {
        // Act
        var result = await _controller.GetAll(null, page, pageSize, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid pagination parameters", ((Result)badRequestResult.Value!).Message);
    }
}
