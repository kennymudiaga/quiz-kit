using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Data;
using QuizKit.Core.RequestHandlers.Users;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Users;

public class UnlockUserCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly UnlockUserCommandHandler _handler;

    public UnlockUserCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        _handler = new UnlockUserCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithLockedUser_UnlocksUser()
    {
        // Arrange
        var email = $"locked-user-{Guid.NewGuid()}@example.com";
        var user = TestUser.Create(email, false, "Locked", "User");
        user.LockOut(DateTime.UtcNow.AddDays(1), "Test lock");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new UnlockUserCommand
        {
            UserId = user.Id,
            Reason = "Test unlock"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedUser = await _context.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Null(updatedUser.LockoutExpiry);
        Assert.Null(updatedUser.LockoutReason);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var command = new UnlockUserCommand
        {
            UserId = "nonexistent",
            Reason = "Test unlock"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.Message);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
