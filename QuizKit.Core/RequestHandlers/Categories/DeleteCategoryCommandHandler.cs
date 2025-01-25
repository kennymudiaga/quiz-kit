using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Requests.Categories;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Categories;

public class DeleteCategoryCommandHandler(QuizDbContext context) : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly QuizDbContext _context = context;

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return Result.NotFound();
        }

        // Check if any quizzes are using this category
        var hasQuizzes = await _context.Quizzes
            .AnyAsync(q => q.CategoryId == request.Id, cancellationToken);

        if (hasQuizzes)
        {
            return Result.BadRequest("Cannot delete category as it is being used by one or more quizzes.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
