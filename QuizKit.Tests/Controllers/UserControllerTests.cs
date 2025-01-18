using System.Net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Controllers;
using QuizKit.Common.Requests.Users;

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

    [Fact]
    public async Task RequestPasswordReset_WithValidEmail_ReturnsNoContent()
    {
        // Arrange
        var resetCommand = new RequestPasswordResetCommand
        {
            Email = "user@example.com"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<RequestPasswordResetCommand>(), default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.RequestPasswordReset(resetCommand);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal((int)HttpStatusCode.NoContent, noContentResult.StatusCode);
    }

    [Fact]
    public async Task RequestPasswordReset_WithLockedAccount_ReturnsForbid()
    {
        // Arrange
        var resetCommand = new RequestPasswordResetCommand
        {
            Email = "locked@example.com"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<RequestPasswordResetCommand>(), default))
            .ReturnsAsync(new Failure("Account is locked", ResultStatus.Forbidden));

        // Act
        var result = await _controller.RequestPasswordReset(resetCommand);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task RequestPasswordReset_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var resetCommand = new RequestPasswordResetCommand
        {
            Email = "invalid-email"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<RequestPasswordResetCommand>(), default))
            .ReturnsAsync(new Failure("Invalid email format", ResultStatus.BadRequest));

        // Act
        var result = await _controller.RequestPasswordReset(resetCommand);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid email format", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task SetPassword_WithInvalidDetails_ReturnsBadRequest()
    {
        // Arrange
        var setPasswordCommand = new SetPasswordCommand();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SetPasswordCommand>(), default))
            .ReturnsAsync(new Failure("Invalid set password details", ResultStatus.BadRequest));

        // Act
        var result = await _controller.SetPassword(setPasswordCommand);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Invalid set password details", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task SetPassword_WithValidDetails_ReturnsNoContent()
    {
        // Arrange
        var setPasswordCommand = new SetPasswordCommand
        {
            Email = "test@example.com",
            Token = "validToken123",
            Password = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SetPasswordCommand>(), default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.SetPassword(setPasswordCommand);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }
}
