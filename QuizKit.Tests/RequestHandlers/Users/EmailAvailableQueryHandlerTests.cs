using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Data;
using QuizKit.Core.RequestHandlers.Users;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Users;

public class EmailAvailableQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _dbContext;
    private readonly EmailAvailableQueryHandler _handler;

    public EmailAvailableQueryHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new QuizDbContext(options);
        _handler = new EmailAvailableQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WithAvailableEmail_ReturnsSuccess()
    {
        // Arrange
        var query = new EmailAvailableQuery { Email = "available@example.com" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = "existing@example.com";
        var user = TestUser.Create(email);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var query = new EmailAvailableQuery { Email = email };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Contains("already in use", result.Message);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
