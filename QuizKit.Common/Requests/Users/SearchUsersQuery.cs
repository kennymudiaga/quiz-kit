using MediatR;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Users;

public record SearchUsersQuery : IRequest<Result<PagedList<UserViewModel>>>
{
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
