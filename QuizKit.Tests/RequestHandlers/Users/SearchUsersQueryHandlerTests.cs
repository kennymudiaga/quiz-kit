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
        var user1 = TestUser.Create("user1@example.com", firstName: "john", lastName: "doe");
        var user2 = TestUser.Create("user2@example.com", firstName: "jane", lastName: "smith");
        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();

        var query = new SearchUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var data = result.Data!;
        Assert.Equal(2, data.TotalCount);
        Assert.Equal(2, data.Items.Count);
        Assert.Equal(1, data.Page);
        Assert.Equal(20, data.PageSize);
    }

    [Theory]
    [InlineData("john")]
    [InlineData("doe")]
    public async Task Handle_WithSearchTerm_ReturnsMatchingUsers(string searchTerm)
    {
        // Arrange
        var user1 = TestUser.Create("user1@example.com", firstName: "john", lastName: "doe");
        var user2 = TestUser.Create("user2@example.com", firstName: "jane", lastName: "smith");
        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();

        var query = new SearchUsersQuery { SearchTerm = searchTerm };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var data = result.Data!;
        Assert.Equal(1, data.TotalCount);
        Assert.Single(data.Items);
        Assert.Equal("john", data.Items[0].FirstName);
        Assert.Equal("doe", data.Items[0].LastName);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var users = Enumerable.Range(1, 25)
            .Select(i => TestUser.Create($"user{i}@example.com", firstName: $"User{i}"))
            .ToList();
        await _dbContext.Users.AddRangeAsync(users);
        await _dbContext.SaveChangesAsync();

        var query = new SearchUsersQuery { Page = 2, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var data = result.Data!;
        Assert.Equal(25, data.TotalCount);
        Assert.Equal(10, data.Items.Count);
        Assert.Equal(2, data.Page);
        Assert.Equal(10, data.PageSize);
        Assert.True(data.HasPreviousPage);
        Assert.True(data.HasNextPage);
    }

    [Fact]
    public async Task Handle_WithNoResults_ReturnsEmptyPage()
    {
        // Arrange
        var query = new SearchUsersQuery { SearchTerm = "nonexistent" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var data = result.Data!;
        Assert.Equal(0, data.TotalCount);
        Assert.Empty(data.Items);
        Assert.Equal(1, data.Page);
        Assert.Equal(20, data.PageSize);
        Assert.False(data.HasNextPage);
        Assert.False(data.HasPreviousPage);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
