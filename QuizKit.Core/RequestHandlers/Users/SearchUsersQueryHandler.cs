using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Users;

public class SearchUsersQueryHandler(QuizDbContext dbContext) : IRequestHandler<SearchUsersQuery, Result<List<UserViewModel>>>
{
    private readonly QuizDbContext _dbContext = dbContext;
    private const int DefaultMaxResults = 10;

    public async Task<Result<List<UserViewModel>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                (u.Email!.Contains(searchTerm)) ||
                (u.FirstName!.Contains(searchTerm)) ||
                (u.LastName!.Contains(searchTerm)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
        }

        var maxResults = request.MaxResults ?? DefaultMaxResults;
        var users = await query
            .Take(maxResults)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        return Result.Success(users);
    }
}
