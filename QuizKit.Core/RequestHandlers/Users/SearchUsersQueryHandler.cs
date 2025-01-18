using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Users;

public class SearchUsersQueryHandler(QuizDbContext dbContext) : IRequestHandler<SearchUsersQuery, Result<PagedList<UserViewModel>>>
{
    private readonly QuizDbContext _dbContext = dbContext;

    public async Task<Result<PagedList<UserViewModel>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
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

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.Email)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        var pagedList = new PagedList<UserViewModel>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result.Success(pagedList);
    }
}
