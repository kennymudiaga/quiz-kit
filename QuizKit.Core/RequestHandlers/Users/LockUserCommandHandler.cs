using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Users;

public class LockUserCommandHandler(QuizDbContext context) : IRequestHandler<LockUserCommand, Result>
{
    private readonly QuizDbContext _context = context;

    public async Task<Result> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return new Failure("User not found.", ResultStatus.BadRequest);
        }

        // TODO: Log this action to UserAudit table

        user.LockOut(request.LockoutExpiry, request.Reason);
        _context.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
