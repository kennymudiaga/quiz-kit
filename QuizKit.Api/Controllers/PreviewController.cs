using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Extensions;
using QuizKit.Common.Constants;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using Swashbuckle.AspNetCore.Annotations;

namespace QuizKit.Api.Controllers;

[ApiController]
[Route("[controller]")]
/// <summary>
/// Controller for accessing public quiz preview information
/// </summary>
/// <remarks>
/// Provides endpoints for retrieving quiz previews without authentication
/// </remarks>
public class PreviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public PreviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of quiz previews
    /// </summary>
    /// <param name="organizationId">Optional organization ID to filter by</param>
    /// <param name="categoryId">Optional category ID to filter by</param>
    /// <param name="searchTerm">Optional search term to filter by quiz title</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>A paginated list of quiz previews</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved quiz previews", typeof(PagedList<QuizPreviewModel>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request parameters", typeof(Result))]
    public async Task<IActionResult> GetQuizPreviews(
        [FromQuery] string? organizationId,
        [FromQuery] string? categoryId,
        [FromQuery] string? searchTerm,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var query = new GetPreviewsQuery
        {
            OrganizationId = organizationId,
            CategoryId = categoryId,
            SearchTerm = searchTerm,
            Page = page.GetValueOrDefault(),
            PageSize = pageSize.GetValueOrDefault(),
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
