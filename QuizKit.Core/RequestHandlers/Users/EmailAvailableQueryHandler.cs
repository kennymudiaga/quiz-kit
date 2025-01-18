using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Users;

public class EmailAvailableQueryHandler(QuizDbContext dbContext) : IRequestHandler<EmailAvailableQuery, Result>
{
    private readonly QuizDbContext _dbContext = dbContext;

    public async Task<Result> Handle(EmailAvailableQuery request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        return exists 
            ? Result.BadRequest("Email is already in use")
            : Result.Success();
    }
}
