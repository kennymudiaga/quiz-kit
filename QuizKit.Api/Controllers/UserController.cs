using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Models.Users;
using QuizKit.Api.Extensions;
using QuizKit.Common.Results;
using QuizKit.Common.Constants;

namespace QuizKit.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class UserController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoggedInUserModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoggedInUserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("set-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("check-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
    {
        var query = new EmailAvailableQuery { Email = email };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policies.Admin)]
    [ProducesResponseType(typeof(Result<List<UserViewModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchUsers([FromQuery] string? searchTerm, [FromQuery] int? maxResults)
    {
        var query = new SearchUsersQuery { SearchTerm = searchTerm, MaxResults = maxResults };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }
}
