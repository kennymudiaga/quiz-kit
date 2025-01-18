using MediatR;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public record SearchUsersQuery : IRequest<Result<List<UserViewModel>>>
{
    public string? SearchTerm { get; init; }
    public int? MaxResults { get; init; }
}
