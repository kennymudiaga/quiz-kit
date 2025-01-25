using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Extensions;
using QuizKit.Common.Constants;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Results;
using Swashbuckle.AspNetCore.Annotations;

namespace QuizKit.Api.Controllers;

[ApiController]
[Authorize(Policies.Admin)]
[Route("quiz/{quizId}/[controller]")]
public class QuestionController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [SwaggerOperation(Summary = "Get all questions for a quiz", Description = "Returns all questions for the specified quiz")]
    [SwaggerResponse(StatusCodes.Status200OK, "Questions retrieved successfully", typeof(List<QuestionModel>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Quiz not found", typeof(Result))]
    public async Task<IActionResult> GetQuestions(string quizId, CancellationToken cancellationToken)
    {
        var query = new GetQuestionsQuery { QuizId = quizId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get a question", Description = "Returns a specific question by ID")]
    [SwaggerResponse(StatusCodes.Status200OK, "Question retrieved successfully", typeof(QuestionModel))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Question not found", typeof(Result))]
    public async Task<IActionResult> Get(string quizId, string id, CancellationToken cancellationToken)
    {
        var query = new GetQuestionQuery { QuizId = quizId, Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new question", Description = "Creates a new question for the specified quiz")]
    [SwaggerResponse(StatusCodes.Status201Created, "Question created successfully", typeof(QuestionModel))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid question data", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Quiz not found", typeof(Result))]
    public async Task<IActionResult> Create(string quizId, [FromBody] CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        command = command with { QuizId = quizId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update a question", Description = "Updates an existing question")]
    [SwaggerResponse(StatusCodes.Status200OK, "Question updated successfully", typeof(QuestionModel))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid question data", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Question not found", typeof(Result))]
    public async Task<IActionResult> Update(string quizId, string id, [FromBody] UpdateQuestionCommand command, CancellationToken cancellationToken)
    {
        command = command with { QuizId = quizId, Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a question", Description = "Deletes a specific question by ID")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Question deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Question not found", typeof(Result))]
    public async Task<IActionResult> Delete(string quizId, string id, CancellationToken cancellationToken)
    {
        var command = new DeleteQuestionCommand { QuizId = quizId, Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
