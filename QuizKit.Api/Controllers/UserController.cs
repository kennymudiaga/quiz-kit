using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Models.Users;
using QuizKit.Api.Extensions;
using QuizKit.Common.Constants;
using QuizKit.Common.Models;
using QuizKit.Common.Results;
using Swashbuckle.AspNetCore.Annotations;

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

    [HttpPost("signup")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Register new user", Description = "Creates a new user account with the provided details")]
    [SwaggerResponse(StatusCodes.Status200OK, "User registered successfully", typeof(Result<LoggedInUserModel>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid registration details", typeof(Result))]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Login user", Description = "Authenticates a user and generates a login token")]
    [SwaggerResponse(StatusCodes.Status200OK, "User logged in successfully", typeof(Result<LoggedInUserModel>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid login credentials", typeof(Result))]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("change-password")]
    [SwaggerOperation(Summary = "Change password", Description = "Changes the password for the currently authenticated user")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Password changed successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid password details", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Request password reset", Description = "Initiates a password reset process for the specified email")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Password reset request processed")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid email address", typeof(Result))]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("set-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Set new password", Description = "Sets a new password using a password reset token")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Password set successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid password reset details", typeof(Result))]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("check-email")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Check email availability", Description = "Checks if an email address is available for registration")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Email is available")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Email is invalid or already taken", typeof(Result))]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
    {
        var query = new EmailAvailableQuery { Email = email };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policies.Admin)]
    [SwaggerOperation(Summary = "Search users", Description = "Searches for users based on a search term with pagination")]
    [SwaggerResponse(StatusCodes.Status200OK, "Users retrieved successfully", typeof(Result<PagedList<UserViewModel>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid search parameters", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to search users")]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new SearchUsersQuery { SearchTerm = searchTerm, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpPost("lock")]
    [Authorize(Roles = Roles.Admin)]
    [SwaggerOperation(Summary = "Lock user account", Description = "Locks a user account, preventing them from logging in")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "User account locked successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid lock details", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to lock accounts")]
    public async Task<IActionResult> Lock([FromBody] LockUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("unlock")]
    [Authorize(Roles = Roles.Admin)]
    [SwaggerOperation(Summary = "Unlock user account", Description = "Unlocks a previously locked user account")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "User account unlocked successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid unlock details", typeof(Result))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User is not authorized to unlock accounts")]
    public async Task<IActionResult> Unlock([FromBody] UnlockUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
