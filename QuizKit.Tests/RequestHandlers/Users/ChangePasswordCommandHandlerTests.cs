using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;
using QuizKit.Core.Options;
using QuizKit.Core.RequestHandlers.Users;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuizKit.Tests.RequestHandlers.Users;

public class ChangePasswordCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _dbContext;
    private readonly Mock<IPasswordHasher<string>> _mockPasswordHasher;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<ChangePasswordCommandHandler>> _mockLogger;
    private readonly UserPolicyOptions _userPolicyOptions;
    private readonly UserProfile _testUser;
    private readonly HttpContext _httpContext;

    public ChangePasswordCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new QuizDbContext(options);

        // Setup mocks
        _mockPasswordHasher = new Mock<IPasswordHasher<string>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<ChangePasswordCommandHandler>>();

        // Setup user policy options
        _userPolicyOptions = new UserPolicyOptions
        {
            MinPasswordLength = 8,
            MaxPasswordLength = 64
        };

        // Create test user
        _testUser = CreateTestUser("test@example.com");
        _dbContext.Users.Add(_testUser);
        _dbContext.SaveChanges();

        // Setup HTTP context with claims
        _httpContext = new DefaultHttpContext();
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, _testUser.Email!)
        }));

        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(_httpContext);
    }

    private UserProfile CreateTestUser(string email)
    {
        var user = new UserProfile(new SignUpCommand
        {
            Email = email,
            FirstName = "Test",
            LastName = "User"
        });
        user.SetPassword("old-hashed-password");
        return user;
    }

    [Fact]
    public async Task Handle_WithValidCurrentPassword_ShouldChangePassword()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "old-password",
            NewPassword = "new-password-123"
        };

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(_testUser.Email!, _testUser.PasswordHash!, command.CurrentPassword))
            .Returns(PasswordVerificationResult.Success);

        _mockPasswordHasher
            .Setup(x => x.HashPassword(_testUser.Email!, command.NewPassword))
            .Returns("new-hashed-password");

        var handler = new ChangePasswordCommandHandler(
            _dbContext, 
            _mockPasswordHasher.Object, 
            _userPolicyOptions, 
            _mockHttpContextAccessor.Object, 
            _mockLogger.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockPasswordHasher.Verify(x => x.HashPassword(_testUser.Email!, command.NewPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_WithIncorrectCurrentPassword_ShouldFail()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "wrong-password",
            NewPassword = "new-password-123"
        };

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(_testUser.Email!, _testUser.PasswordHash!, command.CurrentPassword))
            .Returns(PasswordVerificationResult.Failed);

        var handler = new ChangePasswordCommandHandler(
            _dbContext, 
            _mockPasswordHasher.Object, 
            _userPolicyOptions, 
            _mockHttpContextAccessor.Object, 
            _mockLogger.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.BadRequest, ((Failure)result).Status);
        _mockPasswordHasher.Verify(x => x.HashPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsLocked_ShouldFail()
    {
        // Arrange
        // Create a new user and lock it
        var lockedUser = CreateTestUser("test-locked@example.com");
        lockedUser.LogAccessFailure(true, 3, 30);
        lockedUser.LogAccessFailure(true, 3, 30);
        lockedUser.LogAccessFailure(true, 3, 30);


        _dbContext.Users.Add(lockedUser);
        _dbContext.SaveChanges();

        // Verify the user is actually locked
        Assert.True(lockedUser.IsAccountLocked, "User should be locked");

        // Update the http context with the locked user's email
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, lockedUser.Email!)
        }));

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "old-password",
            NewPassword = "new-password-123"
        };

        var handler = new ChangePasswordCommandHandler(
            _dbContext, 
            _mockPasswordHasher.Object, 
            _userPolicyOptions, 
            _mockHttpContextAccessor.Object, 
            _mockLogger.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task Handle_WithoutEmailClaim_ShouldFail()
    {
        // Arrange
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext!.User)
            .Returns(new ClaimsPrincipal());

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "old-password",
            NewPassword = "new-password-123"
        };

        var handler = new ChangePasswordCommandHandler(
            _dbContext, 
            _mockPasswordHasher.Object, 
            _userPolicyOptions, 
            _mockHttpContextAccessor.Object, 
            _mockLogger.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Unauthorized, ((Failure)result).Status);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
