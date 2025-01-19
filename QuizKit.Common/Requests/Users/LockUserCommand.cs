using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public class LockUserCommand : IRequest<Result>
{
    /// <summary>
    /// The id of the user to lock.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The reason for locking the account.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the lockout will expire. If null, the lockout will be indefinite.
    /// </summary>
    public DateTime? LockoutExpiry { get; set; }
}
