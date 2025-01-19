using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public class UnlockUserCommand : IRequest<Result>
{
    /// <summary>
    /// The id of the user to unlock.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The reason for unlocking the account.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
