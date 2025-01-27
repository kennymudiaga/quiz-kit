using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Controllers;
using QuizKit.Common.Constants;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;

namespace QuizKit.Tests.Controllers;

public class PreviewControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly PreviewController _controller;

    public PreviewControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new PreviewController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetQuizPreviews_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var query = new GetPreviewsQuery
        {
            OrganizationId = "test-org",
            CategoryId = "test-category",
            SearchTerm = "test",
            Page = 1,
            PageSize = 10
        };

        var previews = new List<QuizPreviewModel>
        {
            new()
            {
                Id = "test-id",
                Title = "Test Quiz",
                Description = "Test Description",
                Category = "test-category",
                QuestionsCount = 5
            }
        };

        var pagedList = new PagedList<QuizPreviewModel>
        {
            Items = previews,
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _mockMediator.Setup(m => m.Send(It.Is<GetPreviewsQuery>(q =>
            q.OrganizationId == query.OrganizationId &&
            q.CategoryId == query.CategoryId &&
            q.SearchTerm == query.SearchTerm &&
            q.Page == query.Page &&
            q.PageSize == query.PageSize), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<QuizPreviewModel>>.Success(pagedList));

        // Act
        var result = await _controller.GetQuizPreviews(
            query.OrganizationId,
            query.CategoryId,
            query.SearchTerm,
            query.Page,
            query.PageSize,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedList<QuizPreviewModel>>(okResult.Value);
        Assert.Equal(previews.Count, returnValue.Items.Count);
        Assert.Equal(previews[0].Id, returnValue.Items[0].Id);
        Assert.Equal(previews[0].Title, returnValue.Items[0].Title);
    }

    [Fact]
    public async Task GetQuizPreviews_WithDefaultParameters_ReturnsOk()
    {
        // Arrange
        var pagedList = new PagedList<QuizPreviewModel>
        {
            Items = new List<QuizPreviewModel>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        _mockMediator.Setup(m => m.Send(It.Is<GetPreviewsQuery>(q =>
            q.OrganizationId == null &&
            q.CategoryId == null &&
            q.SearchTerm == null &&
            q.Page == PaginationConstants.DefaultPage &&
            q.PageSize == PaginationConstants.DefaultPageSize), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedList));

        // Act
        var result = await _controller.GetQuizPreviews(
            organizationId: null,
            categoryId: null,
            searchTerm: null,
            page: null,
            pageSize: null,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedList<QuizPreviewModel>>(okResult.Value);
        Assert.Empty(returnValue.Items);
    }

    [Fact]
    public async Task GetQuizPreviews_WhenMediatorReturnsError_ReturnsBadRequest()
    {
        // Arrange
        var errorMessage = "Invalid parameters";
        _mockMediator.Setup(m => m.Send(It.IsAny<GetPreviewsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(errorMessage));

        // Act
        var result = await _controller.GetQuizPreviews(
            organizationId: null,
            categoryId: null,
            searchTerm: null,
            page: null,
            pageSize: null, 
            CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<Result>(badRequestResult.Value);
        Assert.Equal(errorMessage, returnValue.Message);
    }

    [Fact]
    public async Task GetQuizPreview_WithValidId_ReturnsOk()
    {
        // Arrange
        var preview = new QuizPreviewModel
        {
            Id = "test-id",
            Title = "Test Quiz",
            Description = "Test Description",
            Category = "test-category",
            QuestionsCount = 5
        };

        _mockMediator.Setup(m => m.Send(
            It.Is<GetPreviewByIdQuery>(q => q.QuizId == "test-id"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(preview));

        // Act
        var result = await _controller.GetQuizPreview("test-id", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<QuizPreviewModel>(okResult.Value);
        Assert.Equal(preview.Id, returnValue.Id);
    }

    [Fact]
    public async Task GetQuizPreview_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockMediator.Setup(m => m.Send(
            It.IsAny<GetPreviewByIdQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

        // Act
        var result = await _controller.GetQuizPreview("invalid-id", CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetQuizPreview_WithUnavailableQuiz_ReturnsBadRequest()
    {
        // Arrange
        var errorMessage = "Quiz not available";
        _mockMediator.Setup(m => m.Send(
            It.IsAny<GetPreviewByIdQuery>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.BadRequest(errorMessage));

        // Act
        var result = await _controller.GetQuizPreview("draft-quiz-id", CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<Result>(badRequestResult.Value);
        Assert.Equal(errorMessage, returnValue.Message);
    }
}
