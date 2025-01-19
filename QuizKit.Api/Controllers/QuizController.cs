using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Api.Extensions;
using QuizKit.Common.Constants;
using QuizKit.Common.Requests.Quizzes;

namespace QuizKit.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class QuizController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Create a new quiz
    /// </summary>
    [HttpPost]
    [Authorize(Policies.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateQuizCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
