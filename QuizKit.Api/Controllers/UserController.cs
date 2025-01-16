using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Models.Users;

namespace QuizKit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("signup")]
    [ProducesResponseType(typeof(LoggedInUserModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoggedInUserModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(result);
    }
}
