using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizKit.Api.Controllers;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using Xunit;

namespace QuizKit.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockMediator = new Mock<IMediator>();

        _controller = new UserController(
            _mockMediator.Object
        );
    }

    [Fact]
    public async Task SignUp_WithInvalidDetails_ReturnsBadRequest()
    {
        // Arrange
        var signUpCommand = new SignUpCommand();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SignUpCommand>(), default))
            .ReturnsAsync(new Failure("Invalid signup details", ResultStatus.BadRequest));

        // Act
        var result = await _controller.SignUp(signUpCommand);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid signup details", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginCommand = new LoginCommand();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
            .ReturnsAsync(new Failure("Invalid login credentials", ResultStatus.BadRequest));

        // Act
        var result = await _controller.Login(loginCommand);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid login credentials", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task ChangePassword_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var changePasswordCommand = new ChangePasswordCommand
        {
            CurrentPassword = "oldPassword123",
            NewPassword = "newPassword456"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.ChangePassword(changePasswordCommand);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal((int)HttpStatusCode.NoContent, noContentResult.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithInvalidCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var changePasswordCommand = new ChangePasswordCommand
        {
            CurrentPassword = "wrongOldPassword",
            NewPassword = "newPassword456"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), default))
            .ReturnsAsync(new Failure("Invalid current password", ResultStatus.BadRequest));

        // Act
        var result = await _controller.ChangePassword(changePasswordCommand);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid current password", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task ChangePassword_WhenUserIsLocked_ReturnsForbid()
    {
        // Arrange
        var changePasswordCommand = new ChangePasswordCommand
        {
            CurrentPassword = "oldPassword123",
            NewPassword = "newPassword456"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), default))
            .ReturnsAsync(new Failure("Account is locked", ResultStatus.Forbidden));

        // Act
        var result = await _controller.ChangePassword(changePasswordCommand);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}
