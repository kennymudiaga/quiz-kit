using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizKit.Common.Enums;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Data;
using QuizKit.Core.RequestHandlers.Users;
using QuizKit.Core.ServiceContracts;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Users;

public class RequestPasswordResetCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _dbContext;
    private readonly Mock<ITokenGenerator> _mockTokenGenerator;
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
            _mockTokenGenerator.Object, 
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
            _mockTokenGenerator.Object, 
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
    public async Task Handle_WithValidUser_GeneratesResetToken()
    {
        // Arrange
        var user = TestUser.Create("valid@example.com", false);
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        _mockTokenGenerator
            .Setup(t => t.GenerateToken(6, TokenType.Numeric))
            .Returns("123456");

        var handler = new RequestPasswordResetCommandHandler(
            _dbContext, 
            _mockTokenGenerator.Object, 
            _mockLogger.Object);

        var command = new RequestPasswordResetCommand { Email = "valid@example.com" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockTokenGenerator.Verify(
            t => t.GenerateToken(6, TokenType.Numeric), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTokenGenerationFails_ReturnFailureResult()
    {
        // Arrange
        var user = TestUser.Create("valid@example.com", false);
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        _mockTokenGenerator
            .Setup(t => t.GenerateToken(6, TokenType.Numeric))
            .Throws(new Exception("Token generation failed"));

        var handler = new RequestPasswordResetCommandHandler(
            _dbContext, 
            _mockTokenGenerator.Object, 
            _mockLogger.Object);

        var command = new RequestPasswordResetCommand { Email = "valid@example.com" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Failure, result.Status);
        _mockTokenGenerator.Verify(
            t => t.GenerateToken(6, TokenType.Numeric), 
            Times.Once);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
