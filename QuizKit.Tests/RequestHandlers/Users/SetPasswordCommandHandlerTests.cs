using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Data;
using QuizKit.Core.Options;
using QuizKit.Core.RequestHandlers.Users;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Users;

public class SetPasswordCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _dbContext;
    private readonly Mock<IPasswordHasher<string>> _mockPasswordHasher;
    private readonly Mock<ILogger<SetPasswordCommandHandler>> _mockLogger;
    private readonly UserPolicyOptions _userPolicyOptions;

    public SetPasswordCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new QuizDbContext(options);

        // Setup mocks
        _mockPasswordHasher = new Mock<IPasswordHasher<string>>();
        _mockLogger = new Mock<ILogger<SetPasswordCommandHandler>>();
        _userPolicyOptions = new UserPolicyOptions
        {
            EnableLockout = true,
            MaxPasswordFailCount = 3,
            PasswordLockoutDuration = 5,
            PasswordTokenTimeout = 5,
        };
    }

    [Fact]
    public async Task Handle_WithValidTokenAndPassword_ShouldSetPassword()
    {
        // Arrange
        var email = "test@example.com";
        var token = "validToken123456";
        var newPassword = "NewPassword123!";
        var hashedPassword = "hashedPassword123";
        var hashedToken = "hashedToken123";

        var user = TestUser.Create(email);
        user.SetPasswordToken(hashedToken, _userPolicyOptions.PasswordTokenTimeout);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Setup password hasher mocks
        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(email, hashedToken, token))
            .Returns(PasswordVerificationResult.Success);
        _mockPasswordHasher
            .Setup(x => x.HashPassword(email, newPassword))
            .Returns(hashedPassword);

        var handler = new SetPasswordCommandHandler(
            _dbContext, 
            _mockPasswordHasher.Object, 
            _userPolicyOptions, 
            _mockLogger.Object
        );
        var command = new SetPasswordCommand
        {
            Email = email,
            Token = token,
            Password = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        Assert.NotNull(updatedUser);
        Assert.Equal(hashedPassword, updatedUser.PasswordHash);
        Assert.Null(updatedUser.PasswordTokenHash);
        Assert.Null(updatedUser.PasswordTokenExpiry);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldFail()
    {
        // Arrange
        var email = "test@example.com";
        var token = "expiredToken123";
        var hashedToken = "hashedToken123";

        var user = TestUser.Create(email);
        user.SetPasswordToken(hashedToken, -1); // Expired token
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var handler = new SetPasswordCommandHandler(
            _dbContext, 
            _mockPasswordHasher.Object, 
            _userPolicyOptions, 
            _mockLogger.Object
        );
        var command = new SetPasswordCommand
        {
            Email = email,
            Token = token,
            Password = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldFail()
    {
        // Arrange
        var email = "test@example.com";
        var invalidToken = "invalidToken456";
        var hashedToken = "hashedToken123";

        var user = TestUser.Create(email);
        user.SetPasswordToken(hashedToken, _userPolicyOptions.PasswordTokenTimeout);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Setup password hasher mock to simulate invalid token
        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(email, hashedToken, invalidToken))
            .Returns(PasswordVerificationResult.Failed);

        var handler = new SetPasswordCommandHandler(
            _dbContext, 
            _mockPasswordHasher.Object, 
            _userPolicyOptions, 
            _mockLogger.Object
        );
        var command = new SetPasswordCommand
        {
            Email = email,
            Token = invalidToken,
            Password = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldSucceed()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var token = "someToken123";

        var handler = new SetPasswordCommandHandler(
            _dbContext, 
            _mockPasswordHasher.Object, 
            _userPolicyOptions, 
            _mockLogger.Object
        );
        var command = new SetPasswordCommand
        {
            Email = email,
            Token = token,
            Password = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
