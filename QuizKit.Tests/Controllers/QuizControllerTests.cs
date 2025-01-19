using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Controllers;
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
            Id = Guid.NewGuid().ToString(),
            Title = command.Title,
            Description = command.Description,
            OrganizationId = command.OrganizationId,
            Questions = new List<QuestionModel>()
        };
        _mockMediator.Setup(x => x.Send(command, default))
            .ReturnsAsync(Result.Success(quizModel));

        // Act
        var result = await _controller.Create(command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedModel = Assert.IsType<QuizModel>(okResult.Value);
        Assert.Equal(quizModel.Id, returnedModel.Id);
        Assert.Equal(quizModel.Title, returnedModel.Title);
    }

    [Fact]
    public async Task Create_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateQuizCommand
        {
            Title = "Test Quiz",
            Description = "Test Description",
            OrganizationId = "test-org"
        };
        _mockMediator.Setup(x => x.Send(command, default))
            .ReturnsAsync(new Result<QuizModel>{ Message = "Invalid command", Status = ResultStatus.BadRequest });
        // Act
        var result = await _controller.Create(command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var resultData = Assert.IsAssignableFrom<Result>(badRequestResult.Value);
        Assert.Equal("Invalid command", resultData.Message);
    }
}
