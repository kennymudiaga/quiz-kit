using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizKit.Common.Enums;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Data;
using QuizKit.Core.Options;
using QuizKit.Core.RequestHandlers.Users;
using QuizKit.Core.ServiceContracts;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Users;

public class RequestPasswordResetCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _dbContext;
    private readonly UserPolicyOptions _userPolicyOptions = new();
    private readonly Mock<ITokenGenerator> _mockTokenGenerator;
    private readonly Mock<IPasswordHasher<string>> _mockPasswordHasher = new();
    private readonly Mock<ILogger<RequestPasswordResetCommandHandler>> _mockLogger;

    public RequestPasswordResetCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new QuizDbContext(options);

        // Setup mocks
        _mockTokenGenerator = new Mock<ITokenGenerator>();
        _mockLogger = new Mock<ILogger<RequestPasswordResetCommandHandler>>();
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ReturnsSuccessResult()
    {
        // Arrange
        var handler = new RequestPasswordResetCommandHandler(
            _dbContext, 
            _userPolicyOptions,
            _mockTokenGenerator.Object, 
            _mockPasswordHasher.Object,
            _mockLogger.Object);

        var command = new RequestPasswordResetCommand { Email = "nonexistent@example.com" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockTokenGenerator.Verify(t => t.GenerateToken(It.IsAny<int>(), It.IsAny<TokenType>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithLockedAccount_ReturnsForbiddenResult()
    {
        // Arrange
        var lockedUser = TestUser.Create("locked@example.com", true);
        _dbContext.Users.Add(lockedUser);
        _dbContext.SaveChanges();

        var handler = new RequestPasswordResetCommandHandler(
            _dbContext, 
            _userPolicyOptions,
            _mockTokenGenerator.Object, 
            _mockPasswordHasher.Object,
            _mockLogger.Object);

        var command = new RequestPasswordResetCommand { Email = "locked@example.com" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Forbidden, result.Status);
        _mockTokenGenerator.Verify(t => t.GenerateToken(It.IsAny<int>(), It.IsAny<TokenType>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidUser_SavesHashedTokenAndExpiry()
    {
        // Arrange
        var user = TestUser.Create($"valid-{Guid.NewGuid()}@example.com", false);
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var mockToken = "12345678";
        var hashedToken = "hashedToken";
        _mockTokenGenerator
            .Setup(t => t.GenerateToken(8, TokenType.Numeric))
            .Returns(mockToken);

        _mockPasswordHasher
            .Setup(p => p.HashPassword(user.Email!, mockToken))
            .Returns(hashedToken);

        var handler = new RequestPasswordResetCommandHandler(
            _dbContext,
            _userPolicyOptions,
            _mockTokenGenerator.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object);

        var command = new RequestPasswordResetCommand { Email = user.Email! };

        var startTime = DateTime.UtcNow;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockTokenGenerator.Verify(
            t => t.GenerateToken(8, TokenType.Numeric), 
            Times.Once);

        // Reload user from database
        var updatedUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == user.Email);
        Assert.NotNull(updatedUser);
        Assert.Equal(hashedToken, updatedUser.PasswordTokenHash);

        // Check that the token expiry is set
        Assert.NotNull(updatedUser.PasswordTokenExpiry);

        // Check that the token expiry is within the expected range
        var timeElapsed = DateTime.UtcNow - startTime;
        var lowerLimit = startTime.AddMinutes(_userPolicyOptions.PasswordTokenTimeout);
        var upperLimit = lowerLimit.AddTicks(timeElapsed.Ticks);
        Assert.InRange(updatedUser.PasswordTokenExpiry.Value, lowerLimit, upperLimit);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
