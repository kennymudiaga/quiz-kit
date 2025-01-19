using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Controllers;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;

namespace QuizKit.Tests.Controllers;

public class QuizControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly QuizController _controller;

    public QuizControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new QuizController(_mockMediator.Object);
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOk()
    {
        // Arrange
        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            OrganizationId = "test-org"
        };
        var quizModel = new QuizModel
        {
            Id = "test-id",
            Title = command.Title,
            Description = command.Description,
            OrganizationId = command.OrganizationId
        };

        _mockMediator.Setup(m => m.Send(command, default))
            .ReturnsAsync(Result.Success(quizModel));

        // Act
        var result = await _controller.Create(command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<QuizModel>(okResult.Value);
        Assert.Equal(quizModel.Id, returnValue!.Id);
    }

    [Fact]
    public async Task Create_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateQuizCommand();
        _mockMediator.Setup(m => m.Send(command, default))
            .ReturnsAsync(Result.BadRequest("Invalid command"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<Result>(badRequestResult.Value);
        Assert.False(returnValue.IsSuccess);
    }

    [Fact]
    public async Task GetQuizzes_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var query = new GetQuizzesQuery
        {
            OrganizationId = "test-org",
            CategoryId = "test-cat",
            SearchTerm = "test",
            Page = 1,
            PageSize = 10
        };

        var quizzes = new List<QuizModel>
        {
            new() { Id = "quiz1", Title = "Quiz 1" },
            new() { Id = "quiz2", Title = "Quiz 2" }
        };

        var pagedList = new PagedList<QuizModel>
        {
            Items = quizzes,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _mockMediator.Setup(m => m.Send(It.Is<GetQuizzesQuery>(q =>
            q.OrganizationId == query.OrganizationId &&
            q.CategoryId == query.CategoryId &&
            q.SearchTerm == query.SearchTerm &&
            q.Page == query.Page &&
            q.PageSize == query.PageSize), default))
            .ReturnsAsync(Result.Success(pagedList));

        // Act
        var result = await _controller.GetQuizzes(
            query.OrganizationId,
            query.CategoryId,
            query.SearchTerm,
            query.Page,
            query.PageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedList<QuizModel>>(okResult.Value);
        Assert.Equal(2, returnValue!.Items.Count);
        Assert.Equal(2, returnValue.TotalCount);
        Assert.Equal(1, returnValue.Page);
        Assert.Equal(10, returnValue.PageSize);
    }

    [Fact]
    public async Task GetQuizzes_WithInvalidParameters_ReturnsBadRequest()
    {
        // Arrange
        _mockMediator.Setup(m => m.Send(It.IsAny<GetQuizzesQuery>(), default))
            .ReturnsAsync(Result.BadRequest("Invalid parameters"));

        // Act
        var result = await _controller.GetQuizzes(page: -1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<Result>(badRequestResult.Value);
        Assert.False(returnValue.IsSuccess);
    }
}
