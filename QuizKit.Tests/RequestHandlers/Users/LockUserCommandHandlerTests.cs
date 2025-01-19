using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Data;
using QuizKit.Core.RequestHandlers.Users;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Users;

public class LockUserCommandHandlerTests : IDisposable
{
    private readonly QuizDbContext _context;
    private readonly LockUserCommandHandler _handler;

    public LockUserCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuizDbContext(options);
        _handler = new LockUserCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidRequest_LocksUser()
    {
        var email = $"locked-user-{Guid.NewGuid()}@example.com";
        var lockoutExpiry = DateTime.UtcNow.AddHours(1);

        // Arrange
        var user = TestUser.Create(email, false, "Locked", "User");
        var command = new LockUserCommand
        {
            Email = email,
            Reason = "Violation of terms",
            LockoutExpiry = lockoutExpiry,
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var lockedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(lockedUser);
        Assert.True(lockedUser.IsAccountLocked);
        Assert.Equal(command.Reason, lockedUser.LockoutReason);
        Assert.Equal(lockedUser.LockoutExpiry!.Value, lockoutExpiry);
    }

    [Fact]
    public async Task Handle_WithNullLockExpiry_LocksIndefinitely()
    {
        var email = $"locked-user-{Guid.NewGuid()}@example.com";

        // Arrange
        var user = TestUser.Create(email, false, "Locked", "Indefinite");
        var command = new LockUserCommand
        {
            Email = email,
            Reason = "Violation of terms",
        };
        _context.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var lockedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(lockedUser);
        Assert.True(lockedUser.IsAccountLocked);
        Assert.Equal(command.Reason, lockedUser.LockoutReason);
        Assert.Equal(lockedUser.LockoutExpiry!.Value, DateTime.MaxValue);
    }

    [Fact]
    public async Task Handle_WithNonexistentEmail_ReturnsNotFound()
    {
        // Arrange
        var command = new LockUserCommand
        {
            Email = "nonexistent@example.com",
            Reason = "Violation of terms"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.Message);
    }

    [Fact]
    public async Task Handle_WithAlreadyLockedUser_ExtendsLockout()
    {
        var email = $"locked-user-{Guid.NewGuid()}@example.com";
        var lockoutExpiry = DateTime.UtcNow.AddHours(1);

        // Arrange
        var user = TestUser.Create(email, isLocked: true, firstName: "Locked", lastName: "User2");
        _context.Add(user);
        await _context.SaveChangesAsync();

        // verify lockout
        var lockedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(lockedUser!.LockoutExpiry);
        Assert.True(lockedUser.IsAccountLocked);

        var command = new LockUserCommand
        {
            Email = email,
            Reason = "Another violation",
            LockoutExpiry = lockoutExpiry,
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // verify lockout expiry is extended
        lockedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(lockedUser!.LockoutExpiry);
        Assert.Equal(command.Reason, lockedUser.LockoutReason);
        Assert.Equal(lockedUser.LockoutExpiry!.Value, lockoutExpiry);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
