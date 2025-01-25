using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;
using QuizKit.Common.Results;
using QuizKit.Core.Utils;
using Xunit;

namespace QuizKit.IntegrationTests;

public class CategoryControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CategoryControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Create_WithValidCommand_ReturnsCreated()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new CreateCategoryCommand
        {
            Id = IdGenerator.GenerateId(12),
            Description = "Programming related questions"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/category", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var category = await response.Content.ReadFromJsonAsync<CategoryModel>(_jsonOptions);
        Assert.NotNull(category);
        Assert.Equal(command.Id, category!.Id);
        Assert.Equal(command.Description, category.Description);
        Assert.NotEqual(default, category.CreationTime);
    }

    [Fact]
    public async Task Create_WithExistingId_ReturnsBadRequest()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };

        await _client.PostAsJsonAsync("/category", command);
        
        // Act
        var response = await _client.PostAsJsonAsync("/category", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
        Assert.NotNull(result);
        Assert.False(result!.IsSuccess);
        Assert.Equal("A category with this ID already exists.", result.Message);
    }

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOk()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var createCommand = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };
        await _client.PostAsJsonAsync("/category", createCommand);

        var updateCommand = new UpdateCategoryCommand
        {
            Id = "programming",
            Description = "Updated programming questions"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/category/{createCommand.Id}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var category = await response.Content.ReadFromJsonAsync<CategoryModel>(_jsonOptions);
        Assert.NotNull(category);
        Assert.Equal(updateCommand.Id, category!.Id);
        Assert.Equal(updateCommand.Description, category.Description);
        Assert.NotNull(category.LastUpdateTime);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new UpdateCategoryCommand
        {
            Id = "nonexistent",
            Description = "Updated description"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/category/nonexistent", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };
        await _client.PostAsJsonAsync("/category", command);

        // Act
        var response = await _client.DeleteAsync($"/category/{command.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify category is deleted
        var getResponse = await _client.GetAsync($"/category/{command.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        // Act
        var response = await _client.DeleteAsync("/category/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidId_ReturnsCategory()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };
        await _client.PostAsJsonAsync("/category", command);

        // Act
        var response = await _client.GetAsync($"/category/{command.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var category = await response.Content.ReadFromJsonAsync<CategoryModel>(_jsonOptions);
        Assert.NotNull(category);
        Assert.Equal(command.Id, category!.Id);
        Assert.Equal(command.Description, category.Description);
    }

    [Fact]
    public async Task GetAll_WithNoFilters_ReturnsAllCategories()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var categories = new[]
        {
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = "Programming" },
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = "History" },
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = "Science" }
        };

        foreach (var category in categories)
        {
            await _client.PostAsJsonAsync("/category", category);
        }

        // Act
        var response = await _client.GetAsync("/category?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<CategoryModel>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result!.Items.Count >= 3, "Expected at least 3 categories.");
    }

    [Fact]
    public async Task GetAll_WithSearchTerm_ReturnsMatchingCategories()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
        var marker = IdGenerator.GenerateId(8);

        var categories = new[]
        {
            new CreateCategoryCommand { Id = $"{marker}_programming", Description = "Programming" },
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = $"Programming {marker}" },
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = $"{marker} History" },
            new CreateCategoryCommand { Id = $"history_{marker}", Description = "Marker on end of Id" },
            new CreateCategoryCommand { Id = $"wrap_{marker}_around", Description = "Wrap marker around" },
        };

        foreach (var category in categories)
        {
            await _client.PostAsJsonAsync("/category", category);
        }

        // Act
        var response = await _client.GetAsync($"/category?searchTerm={marker}&page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<CategoryModel>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");

        var marker = IdGenerator.GenerateId(8);
        var categories = new[]
        {
            new CreateCategoryCommand { Id = $"{marker}_programming", Description = "Programming" },
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = $"Programming {marker}" },
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = $"{marker} History" },
            new CreateCategoryCommand { Id = $"history_{marker}", Description = "Marker on end of Id" },
            new CreateCategoryCommand { Id = $"wrap_{marker}_around", Description = "Wrap marker around" },
        };

        foreach (var category in categories)
        {
            await _client.PostAsJsonAsync("/category", category);
        }

        // Act
        var response = await _client.GetAsync("/category?page=2&pageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<CategoryModel>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 5, "Expected TotalCount if at least 5.");
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task Create_AsBasicUser_ReturnsForbidden()
    {
        // Arrange
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/category", command);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_AsBasicUser_ReturnsForbidden()
    {
        // Arrange
        // Create category as admin
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
        var createCommand = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };
        await _client.PostAsJsonAsync("/category", createCommand);

        // Try to update as basic user
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");
        var updateCommand = new UpdateCategoryCommand
        {
            Id = "programming",
            Description = "Updated description"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/category/{updateCommand.Id}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsBasicUser_ReturnsForbidden()
    {
        // Arrange
        // Create category as admin
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };
        await _client.PostAsJsonAsync("/category", command);

        // Try to delete as basic user
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        // Act
        var response = await _client.DeleteAsync($"/category/{command.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsBasicUser_ReturnsOk()
    {
        // Arrange
        // Create category as admin
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
        var command = new CreateCategoryCommand
        {
            Id = "programming",
            Description = "Programming related questions"
        };
        await _client.PostAsJsonAsync("/category", command);

        // Get as basic user
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        // Act
        var response = await _client.GetAsync($"/category/{command.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryModel>(_jsonOptions);
        Assert.NotNull(category);
        Assert.Equal(command.Id, category!.Id);
        Assert.Equal(command.Description, category.Description);
    }

    [Fact]
    public async Task GetAll_AsBasicUser_ReturnsOk()
    {
        // Arrange
        // Create categories as admin
        await _client.LoginAsync(LoginTestUserBehavior.AdminUserEmail, "StrongPassword123!");
        var categories = new[]
        {
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = "Programming" },
            new CreateCategoryCommand { Id = IdGenerator.GenerateId(), Description = "History" }
        };
        foreach (var category in categories)
        {
            await _client.PostAsJsonAsync("/category", category);
        }

        // Get as basic user
        await _client.LoginAsync(LoginTestUserBehavior.BasicUserEmail, "StrongPassword123!");

        // Act
        var response = await _client.GetAsync("/category?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedList<CategoryModel>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Items.Count >= 2, "Expected at least 2 categories.");
    }
}
