using System.Net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuizKit.Api.Controllers;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;

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

    [Fact]
    public async Task CheckEmailAvailability_WithAvailableEmail_ReturnsNoContent()
    {
        // Arrange
        var email = "available@example.com";
        _mockMediator
            .Setup(m => m.Send(It.Is<EmailAvailableQuery>(q => q.Email == email), default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.CheckEmailAvailability(email);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CheckEmailAvailability_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = "existing@example.com";
        _mockMediator
            .Setup(m => m.Send(It.Is<EmailAvailableQuery>(q => q.Email == email), default))
            .ReturnsAsync(Result.BadRequest("Email is already in use"));

        // Act
        var result = await _controller.CheckEmailAvailability(email);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Email is already in use", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task CheckEmailAvailability_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = "invalid-email";
        _mockMediator
            .Setup(m => m.Send(It.Is<EmailAvailableQuery>(q => q.Email == email), default))
            .ReturnsAsync(Result.BadRequest("Please provide a valid email address"));

        // Act
        var result = await _controller.CheckEmailAvailability(email);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, badRequestResult.StatusCode);
        Assert.Equal("Please provide a valid email address", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task SearchUsers_WithValidQuery_ReturnsOkWithResults()
    {
        // Arrange
        var users = new List<UserViewModel>
        {
            new() { Id = "1", Email = "user1@example.com", FirstName = "john", LastName = "doe", PhoneNumber = "1234567890" }
        };
        var pagedList = new PagedList<UserViewModel>
        {
            Items = users,
            Page = 1,
            PageSize = 20,
            TotalCount = 1
        };
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SearchUsersQuery>(), default))
            .ReturnsAsync(Result.Success(pagedList));

        // Act
        var result = await _controller.SearchUsers("john", 1, 20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<PagedList<UserViewModel>>(okResult.Value);
        Assert.Single(data.Items);
        Assert.Equal(1, data.TotalCount);
    }

    [Theory]
    [InlineData(0, 20)]  // Invalid page
    [InlineData(1, 0)]   // Invalid page size
    [InlineData(1, 101)] // Page size too large
    public async Task SearchUsers_WithInvalidParameters_ReturnsBadRequest(int page, int pageSize)
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SearchUsersQuery>(), default))
            .ReturnsAsync(() => new Result<PagedList<UserViewModel>> { Message = "Invalid pagination parameters", Status = ResultStatus.BadRequest });

        // Act
        var result = await _controller.SearchUsers(null, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid pagination parameters", ((Result)badRequestResult.Value!).Message);
    }

    [Fact]
    public async Task Lock_WithValidCommand_ReturnsNoContent()
    {
        // Arrange
        var command = new LockUserCommand
        {
            UserId = "test@example.com",
            Reason = "Suspicious activity",
            LockoutExpiry = DateTime.UtcNow.AddDays(7)
        };
        _mockMediator.Setup(x => x.Send(command, default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Lock(command);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Fact]
    public async Task Lock_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new LockUserCommand
        {
            UserId = "test@example.com",
            Reason = "Suspicious activity"
        };
        _mockMediator.Setup(x => x.Send(command, default))
            .ReturnsAsync(new Failure("Invalid command", ResultStatus.BadRequest));

        // Act
        var result = await _controller.Lock(command);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Lock_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var command = new LockUserCommand
        {
            UserId = "nonexistent@example.com",
            Reason = "Suspicious activity"
        };
        _mockMediator.Setup(x => x.Send(command, default))
            .ReturnsAsync(Result.BadRequest("User not found."));

        // Act
        var result = await _controller.Lock(command);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Unlock_WithValidCommand_ReturnsNoContent()
    {
        // Arrange
        var command = new UnlockUserCommand
        {
            UserId = "test@example.com",
            Reason = "Account verified"
        };
        _mockMediator.Setup(x => x.Send(command, default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Unlock(command);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Unlock_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new UnlockUserCommand
        {
            UserId = "test@example.com",
            Reason = "Account verified"
        };
        _mockMediator.Setup(x => x.Send(command, default))
            .ReturnsAsync(new Failure("Invalid command", ResultStatus.BadRequest));

        // Act
        var result = await _controller.Unlock(command);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Unlock_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var command = new UnlockUserCommand
        {
            UserId = "nonexistent@example.com",
            Reason = "Account verified"
        };
        _mockMediator.Setup(x => x.Send(command, default))
            .ReturnsAsync(Result.BadRequest("User not found."));

        // Act
        var result = await _controller.Unlock(command);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
