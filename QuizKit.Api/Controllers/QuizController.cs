using MediatR;
using Microsoft.AspNetCore.Authorization;
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
/// Controller for managing quiz-related operations
/// </summary>
/// <remarks>
/// Provides endpoints for creating, retrieving, and managing quizzes
/// </remarks>
public class QuizController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    [Authorize(Policies.Admin)]
    [SwaggerOperation(Summary = "Create a new quiz", Description = "Creates a new quiz with the specified details")]
    [SwaggerResponse(StatusCodes.Status200OK, "Quiz created successfully", typeof(Result<QuizModel>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid quiz details provided", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to create quizzes")]
    public async Task<IActionResult> Create([FromBody] CreateQuizCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get quizzes", Description = "Retrieves a paginated list of quizzes with optional filtering")]
    [SwaggerResponse(StatusCodes.Status200OK, "Quizzes retrieved successfully", typeof(Result<PagedList<QuizModel>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid parameters provided", typeof(Result))]
    public async Task<IActionResult> GetQuizzes(
        [FromQuery] string? organizationId = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetQuizzesQuery
        {
            OrganizationId = organizationId,
            CategoryId = categoryId,
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get a quiz by ID", Description = "Retrieves a specific quiz by its ID")]
    [SwaggerResponse(StatusCodes.Status200OK, "Quiz retrieved successfully", typeof(QuizModel))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Quiz not found", typeof(Result))]
    public async Task<IActionResult> Get(string id)
    {
        var query = new GetQuizQuery { Id = id };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [Authorize(Policies.Admin)]
    [SwaggerOperation(Summary = "Update an existing quiz", Description = "Updates an existing quiz with the specified details")]
    [SwaggerResponse(StatusCodes.Status200OK, "Quiz updated successfully", typeof(Result<QuizModel>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid quiz details provided", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Quiz not found", typeof(Result))]
    public async Task<IActionResult> Update(string id, UpdateQuizCommand command)
    {
        if (id != command.Id)
        {
            return Result.BadRequest("ID in URL must match ID in request body.").ToActionResult();
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize(Policies.Admin)]
    [SwaggerOperation(Summary = "Delete a quiz", Description = "Deletes an existing quiz")]
    [SwaggerResponse(StatusCodes.Status200OK, "Quiz deleted successfully", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Quiz not found", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized")]
    public async Task<IActionResult> Delete(string id)
    {
        var command = new DeleteQuizCommand { Id = id };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
