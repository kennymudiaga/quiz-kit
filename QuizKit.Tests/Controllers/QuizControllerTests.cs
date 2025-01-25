using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizKit.Api.Controllers;
using QuizKit.Common.Enums;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;

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
            OrganizationId = command.OrganizationId,
            Status = QuizStatus.Created
        };

        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(quizModel));

        // Act
        var result = await _controller.Create(command, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<QuizModel>(okResult.Value);
        Assert.Equal(quizModel.Id, returnValue.Id);
        Assert.Equal(QuizStatus.Created, returnValue.Status);
    }

    [Fact]
    public async Task Create_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateQuizCommand();
        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Invalid command"));

        // Act
        var result = await _controller.Create(command, default);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetQuizzes_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var quizzes = new List<QuizModel>
        {
            new() { Id = "1", Title = "Quiz 1" },
            new() { Id = "2", Title = "Quiz 2" }
        };

        var pagedList = new PagedList<QuizModel>
        {
            Items = quizzes,
            Page = 1,
            PageSize = 10,
            TotalCount = quizzes.Count
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetQuizzesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedList));

        // Act
        var result = await _controller.GetQuizzes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedList<QuizModel>>(okResult.Value);
        Assert.Equal(2, returnValue.TotalCount);
    }

    [Fact]
    public async Task GetQuizzes_WithStatusFilter_ReturnsFilteredQuizzes()
    {
        // Arrange
        var quizzes = new List<QuizModel>
        {
            new() { Id = "1", Title = "Quiz 1", Status = QuizStatus.Live },
            new() { Id = "2", Title = "Quiz 2", Status = QuizStatus.Live }
        };

        var pagedList = new PagedList<QuizModel>
        {
            Items = quizzes,
            Page = 1,
            PageSize = 10,
            TotalCount = quizzes.Count
        };

        _mockMediator.Setup(m => m.Send(
            It.Is<GetQuizzesQuery>(q => q.Status == QuizStatus.Live),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedList));

        // Act
        var result = await _controller.GetQuizzes(status: QuizStatus.Live, page: 1, pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedList<QuizModel>>(okResult.Value);
        Assert.Equal(2, returnValue.TotalCount);
        Assert.All(returnValue.Items, quiz => Assert.Equal(QuizStatus.Live, quiz.Status));
    }

    [Fact]
    public async Task GetQuizzes_WithStatusAndSearchTerm_ReturnsCombinedFilteredQuizzes()
    {
        // Arrange
        var quizzes = new List<QuizModel>
        {
            new() { Id = "1", Title = "Math Quiz", Status = QuizStatus.Live },
        };

        var pagedList = new PagedList<QuizModel>
        {
            Items = quizzes,
            Page = 1,
            PageSize = 10,
            TotalCount = quizzes.Count
        };

        _mockMediator.Setup(m => m.Send(
            It.Is<GetQuizzesQuery>(q => 
                q.Status == QuizStatus.Live && 
                q.SearchTerm == "Math"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedList));

        // Act
        var result = await _controller.GetQuizzes(
            searchTerm: "Math",
            status: QuizStatus.Live,
            page: 1,
            pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedList<QuizModel>>(okResult.Value);
        Assert.Single(returnValue.Items);
        Assert.Equal("Math Quiz", returnValue.Items[0].Title);
        Assert.Equal(QuizStatus.Live, returnValue.Items[0].Status);
    }

    [Fact]
    public async Task GetQuizzes_WithInvalidParameters_ReturnsBadRequest()
    {
        // Arrange
        _mockMediator.Setup(m => m.Send(It.IsAny<GetQuizzesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.BadRequest("Invalid parameters"));

        // Act
        var result = await _controller.GetQuizzes(page: -1, cancellationToken: default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<Result>(badRequestResult.Value);
        Assert.False(returnValue.IsSuccess);
    }
}
