using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Categories;

public class UpdateCategoryCommandHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<UpdateCategoryCommand, Result<CategoryModel>>
{
    private readonly QuizDbContext _context = context;

    public async Task<Result<CategoryModel>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return Result.NotFound();
        }

        category.Description = request.Description;
        category.LastUpdateTime = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<CategoryModel>(category));
    }
}
