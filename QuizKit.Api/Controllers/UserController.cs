using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Models.Users;
using QuizKit.Api.Extensions;
using QuizKit.Common.Constants;
using QuizKit.Common.Models;

namespace QuizKit.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
/// <summary>
/// Controller for managing user-related operations
/// </summary>
/// <remarks>
/// Provides endpoints for user authentication, registration, and account management
/// </remarks>
public class UserController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="command">User signup details</param>
    /// <returns>Created user details or error</returns>
    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoggedInUserModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Authenticate a user and generate login token
    /// </summary>
    /// <param name="command">User login credentials</param>
    /// <returns>Logged in user details or error</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoggedInUserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Change the password for the current user
    /// </summary>
    /// <param name="command">New password details</param>
    /// <returns>No content or error</returns>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Request a password reset for a user
    /// </summary>
    /// <param name="command">User email address</param>
    /// <returns>No content or error</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Set a new password for a user after a password reset
    /// </summary>
    /// <param name="command">New password details</param>
    /// <returns>No content or error</returns>
    [HttpPost("set-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Check if an email address is available for registration
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <returns>No content or error</returns>
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

    /// <summary>
    /// Search for users based on a search term
    /// </summary>
    /// <param name="searchTerm">Search term to filter users</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of users per page</param>
    /// <returns>Search results or error</returns>
    [HttpGet("search")]
    [Authorize(Policies.Admin)]
    [ProducesResponseType(typeof(PagedList<UserViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchUsers([FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new SearchUsersQuery { SearchTerm = searchTerm, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Lock a user account
    /// </summary>
    /// <param name="command">Lock user command details</param>
    /// <returns>Success or error result</returns>
    [HttpPost("lock")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Lock([FromBody] LockUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Unlock a user account
    /// </summary>
    [HttpPost("unlock")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Unlock([FromBody] UnlockUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
