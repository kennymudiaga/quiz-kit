using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Extensions;
using QuizKit.Common.Constants;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;
using QuizKit.Common.Results;
using Swashbuckle.AspNetCore.Annotations;

namespace QuizKit.Api.Controllers;

[ApiController]
[Authorize(Policies.Admin)]
[Route("[controller]")]
/// <summary>
/// Controller for managing quiz categories
/// </summary>
/// <remarks>
/// Provides endpoints for creating, retrieving, updating and deleting quiz categories
/// </remarks>
public class CategoryController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new category", Description = "Creates a new quiz category with the specified details")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Category created successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid category details provided", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to manage categories")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update a category", Description = "Updates an existing quiz category with the specified details")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Category updated successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid category details or ID mismatch", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to manage categories")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(Result))]
    public async Task<IActionResult> Update(
        [SwaggerParameter("ID of the category to update")] string id, 
        [FromBody] UpdateCategoryCommand command, 
        CancellationToken cancellationToken = default)
    {
        if (command.Id != id)
        {
            return BadRequest(Result.BadRequest("Category ID mismatch"));
        }

        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a category", Description = "Deletes an existing quiz category")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Category deleted successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid category ID", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to manage categories")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(Result))]
    public async Task<IActionResult> Delete(
        [SwaggerParameter("ID of the category to delete")] string id, 
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand { Id = id };
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get a category", Description = "Retrieves a specific quiz category by ID")]
    [SwaggerResponse(StatusCodes.Status200OK, "Category retrieved successfully", typeof(Result<CategoryModel>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid category ID", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(Result))]
    public async Task<IActionResult> Get(
        [SwaggerParameter("ID of the category to retrieve")] string id, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryQuery { Id = id };
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get categories", Description = "Retrieves a paginated list of quiz categories with optional search")]
    [SwaggerResponse(StatusCodes.Status200OK, "Categories retrieved successfully", typeof(Result<PagedList<CategoryModel>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid parameters provided", typeof(Result))]
    public async Task<IActionResult> GetAll(
        [FromQuery][SwaggerParameter("Optional search term to filter categories")] string? searchTerm,
        [FromQuery][SwaggerParameter("Page number (minimum: 1)")] int page = 1,
        [FromQuery][SwaggerParameter("Page size (minimum: 1, maximum: 100)")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(Result.BadRequest("Invalid pagination parameters"));
        }

        var query = new GetCategoriesQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        };

        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
