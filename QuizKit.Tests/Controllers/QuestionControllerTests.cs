using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Controllers;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;

namespace QuizKit.Tests.Controllers;

public class QuestionControllerTests
{
    private readonly Mock<IMediator> _mediator;
    private readonly QuestionController _controller;

    public QuestionControllerTests()
    {
        _mediator = new Mock<IMediator>();
        _controller = new QuestionController(_mediator.Object);
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOk()
    {
        // Arrange
        var quizId = "test-quiz";
        var command = new CreateQuestionCommand
        {
            QuizId = "will-be-overwritten",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        var expectedModel = new QuestionModel
        {
            Id = 1,
            QuizId = quizId,
            QuestionText = command.QuestionText,
            A = command.A,
            B = command.B,
            C = command.C,
            D = command.D,
            Answer = command.Answer
        };

        _mediator.Setup(m => m.Send(
            It.Is<CreateQuestionCommand>(c => c.QuizId == quizId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedModel));

        // Act
        var result = await _controller.Create(quizId, command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var model = Assert.IsType<QuestionModel>(okResult.Value);
        Assert.Equal(expectedModel.Id, model.Id);
        Assert.Equal(expectedModel.QuizId, model.QuizId);
        Assert.Equal(expectedModel.QuestionText, model.QuestionText);
        Assert.Equal(expectedModel.Answer, model.Answer);

        _mediator.Verify(m => m.Send(
            It.Is<CreateQuestionCommand>(c => c.QuizId == quizId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var quizId = "test-quiz";
        var command = new CreateQuestionCommand
        {
            QuizId = "will-be-overwritten",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        _mediator.Setup(m => m.Send(
            It.Is<CreateQuestionCommand>(c => c.QuizId == quizId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.BadRequest("Invalid command"));

        // Act
        var result = await _controller.Create(quizId, command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResult = Assert.IsAssignableFrom<Result>(badRequestResult.Value);
        Assert.Equal("Invalid command", errorResult.Message);
    }

    [Fact]
    public async Task Create_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        var quizId = "non-existent-quiz";
        var command = new CreateQuestionCommand
        {
            QuizId = "will-be-overwritten",
            QuestionText = "Test Question",
            A = "Option A",
            B = "Option B",
            C = "Option C",
            D = "Option D",
            Answer = "A"
        };

        _mediator.Setup(m => m.Send(
            It.Is<CreateQuestionCommand>(c => c.QuizId == quizId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Quiz not found."));

        // Act
        var result = await _controller.Create(quizId, command);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
    }
}
