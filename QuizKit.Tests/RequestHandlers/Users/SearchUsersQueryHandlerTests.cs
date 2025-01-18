using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Users;
using QuizKit.Core.Data;
using QuizKit.Core.RequestHandlers.Users;
using QuizKit.Tests.TestHelpers;

namespace QuizKit.Tests.RequestHandlers.Users;

public class SearchUsersQueryHandlerTests : IDisposable
{
    private readonly QuizDbContext _dbContext;
    private readonly SearchUsersQueryHandler _handler;

    public SearchUsersQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new QuizDbContext(options);
        _handler = new SearchUsersQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WithNoSearchTerm_ReturnsAllUsers()
    {
        // Arrange
        var user1 = TestUser.Create("user1@example.com", firstName: "John", lastName: "Doe");
        var user2 = TestUser.Create("user2@example.com", firstName: "Jane", lastName: "Smith");
        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();

        var query = new SearchUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var users = result.Data!.ToList();
        Assert.Equal(2, users.Count);
    }

    [Theory]
    [InlineData("john")]
    [InlineData("doe")]
    [InlineData("user1@example")]
    public async Task Handle_WithSearchTerm_ReturnsMatchingUsers(string searchTerm)
    {
        // Arrange
        var user1 = TestUser.Create("user1@example.com", firstName: "John", lastName: "Doe");
        var user2 = TestUser.Create("user2@example.com", firstName: "Jane", lastName: "Smith");
        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();

        var query = new SearchUsersQuery { SearchTerm = searchTerm };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var users = result.Data!.ToList();
        Assert.Single(users);
        Assert.Equal("john", users[0].FirstName);
        Assert.Equal("doe", users[0].LastName);
    }

    [Fact]
    public async Task Handle_WithMaxResults_LimitsResults()
    {
        // Arrange
        var users = Enumerable.Range(1, 5)
            .Select(i => TestUser.Create($"user{i}@example.com", firstName: $"User{i}"))
            .ToList();
        await _dbContext.Users.AddRangeAsync(users);
        await _dbContext.SaveChangesAsync();

        var query = new SearchUsersQuery { MaxResults = 3 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var returnedUsers = result.Data!.ToList();
        Assert.Equal(3, returnedUsers.Count);
    }

    [Fact]
    public async Task Handle_WithNoResults_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchUsersQuery { SearchTerm = "nonexistent" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
