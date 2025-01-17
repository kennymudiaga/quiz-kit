using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizKit.Api.Controllers;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;

namespace QuizKit.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UserController(_mediatorMock.Object);
    }

    [Fact]
    public async Task SignUp_WithValidCommand_ReturnsOkResult()
    {
        // Arrange
        var signUpCommand = new SignUpCommand 
        { 
            Email = "test@example.com", 
            Password = "StrongPassword123!" 
        };
        var expectedUserModel = new LoggedInUserModel 
        { 
            Id = "1", 
            Email = signUpCommand.Email 
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SignUpCommand>(), default))
            .Returns(Task.FromResult(Result<LoggedInUserModel>.Success(expectedUserModel)));

        // Act
        var result = await _controller.SignUp(signUpCommand);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        Assert.Equal(expectedUserModel, okResult.Value);
    }

    [Fact]
    public async Task SignUp_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var signUpCommand = new SignUpCommand 
        { 
            Email = "invalid", 
            Password = "weak" 
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SignUpCommand>(), default))
            .ReturnsAsync(() => new Result<LoggedInUserModel>() { Message = "Invalid signup details", Status = ResultStatus.Failure });

        // Act
        var result = await _controller.SignUp(signUpCommand);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid signup details", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var loginCommand = new LoginCommand 
        { 
            Email = "test@example.com", 
            Password = "ValidPassword123!" 
        };
        var expectedUserModel = new LoggedInUserModel 
        { 
            Id = "1", 
            Email = loginCommand.Email 
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
            .Returns(Task.FromResult(Result<LoggedInUserModel>.Success(expectedUserModel)));

        // Act
        var result = await _controller.Login(loginCommand);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
        Assert.Equal(expectedUserModel, okResult.Value);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginCommand = new LoginCommand 
        { 
            Email = "test@example.com", 
            Password = "WrongPassword" 
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
            .ReturnsAsync(() => new Result<LoggedInUserModel>() { Message = "Invalid login credentials", Status = ResultStatus.Failure });

        // Act
        var result = await _controller.Login(loginCommand);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid login credentials", ((Result)badRequestResult.Value!).Message);
    }
}
