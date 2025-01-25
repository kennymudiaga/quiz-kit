using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Categories;

public class GetCategoryQueryHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<GetCategoryQuery, Result<CategoryModel>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<CategoryModel>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return Result.NotFound();
        }

        var categoryModel = _mapper.Map<CategoryModel>(category);
        return Result.Success(categoryModel);
    }
}
