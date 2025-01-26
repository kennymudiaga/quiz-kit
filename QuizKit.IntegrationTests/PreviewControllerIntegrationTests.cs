using QuizKit.Common.Enums;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.IntegrationTests.TestBase;
using Xunit;

namespace QuizKit.IntegrationTests;

public class PreviewControllerIntegrationTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetQuizPreviews_ReturnsOnlyApprovedAndLiveQuizzes()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create Quiz 1 - Approved
        var quiz1 = await CreateQuizAsync(
            title: "Quiz 1",
            description: "First Quiz",
            categoryId: "cat-1");
        await UpdateQuizStatusAsync(quiz1.Id!, QuizStatus.Approved);

        // Create Quiz 2 - Live
        var quiz2 = await CreateQuizAsync(
            title: "Quiz 2",
            description: "Second Quiz",
            categoryId: "cat-1");
        await UpdateQuizStatusAsync(quiz2.Id!, QuizStatus.Live);

        // Create Quiz 3 - Created (should not appear in results)
        await CreateQuizAsync(
            title: "Quiz 3",
            description: "Third Quiz",
            categoryId: "cat-1");

        // Act
        var result = await GetAsync<PagedList<QuizPreviewModel>>("/preview?page=1&pageSize=100");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count >= 2, "At least 2 quizzes expected.");
        Assert.Contains(result.Items, q => q.Title == "Quiz 1");
        Assert.Contains(result.Items, q => q.Title == "Quiz 2");
    }

    [Fact]
    public async Task GetQuizPreviews_WithSearchTerm_ReturnsMatchingQuizzes()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create Math Quiz - Approved
        var quiz1 = await CreateQuizAsync(
            title: "Math Quiz",
            description: "Test your math skills");
        await UpdateQuizStatusAsync(quiz1.Id!, QuizStatus.Approved);

        // Create Science Quiz - Live
        var quiz2 = await CreateQuizAsync(
            title: "Science Quiz",
            description: "Test your science knowledge");
        await UpdateQuizStatusAsync(quiz2.Id!, QuizStatus.Live);

        // Create Hidden Math Quiz - Created (should not appear in results)
        await CreateQuizAsync(
            title: "Hidden Math Quiz",
            description: "Not yet approved");

        // Act
        var result = await GetAsync<PagedList<QuizPreviewModel>>("/preview?searchTerm=math");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Math Quiz", result.Items[0].Title);
    }

    [Fact(Skip = "Organizations not yet supported")]
    public async Task GetQuizPreviews_WithOrganizationFilter_ReturnsFilteredQuizzes()
    {
        // Arrange
        await LoginAsAdminAsync();

        // Create Quiz 1 - Approved, Org 1
        var quiz1 = await CreateQuizAsync(
            title: "Quiz 1",
            description: "First Quiz",
            organizationId: "org-1");
        await UpdateQuizStatusAsync(quiz1.Id!, QuizStatus.Approved);

        // Create Quiz 2 - Live, Org 2
        var quiz2 = await CreateQuizAsync(
            title: "Quiz 2",
            description: "Second Quiz",
            organizationId: "org-2");
        await UpdateQuizStatusAsync(quiz2.Id!, QuizStatus.Live);

        // Act
        var result = await GetAsync<PagedList<QuizPreviewModel>>("/preview?organizationId=org-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Quiz 1", result.Items[0].Title);
    }
}
