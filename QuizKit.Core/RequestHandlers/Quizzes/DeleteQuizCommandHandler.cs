using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Quizzes;

public class DeleteQuizCommandHandler(QuizDbContext context) : IRequestHandler<DeleteQuizCommand, Result>
{
    private readonly QuizDbContext _context = context;

    public async Task<Result> Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (quiz == null)
        {
            return Result.NotFound("Quiz not found.");
        }

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
