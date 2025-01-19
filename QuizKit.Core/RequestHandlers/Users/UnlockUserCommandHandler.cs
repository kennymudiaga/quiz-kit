using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Users;

public class UnlockUserCommandHandler(QuizDbContext context) : IRequestHandler<UnlockUserCommand, Result>
{
    private readonly QuizDbContext _context = context;

    public async Task<Result> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.BadRequest("User not found.");
        }

        // TODO: Log this action to UserAudit table

        user.EndLockout();
        _context.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
