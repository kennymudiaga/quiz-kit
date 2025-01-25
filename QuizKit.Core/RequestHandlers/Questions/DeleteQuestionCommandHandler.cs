using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Questions;

public class DeleteQuestionCommandHandler(QuizDbContext context) : IRequestHandler<DeleteQuestionCommand, Result>
{
    private readonly QuizDbContext _context = context;

    public async Task<Result> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.QuizQuestions
            .FirstOrDefaultAsync(q => q.Id == request.Id && q.QuizId == request.QuizId, cancellationToken);

        if (question == null)
        {
            return Result.NotFound();
        }

        _context.QuizQuestions.Remove(question);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
