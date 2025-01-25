using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;

namespace QuizKit.Core.RequestHandlers.Categories;

public class CreateCategoryCommandHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<CreateCategoryCommand, Result<CategoryModel>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<CategoryModel>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return Result.BadRequest("Category ID is required.");
        }

        var exists = await _context.Categories
            .AnyAsync(c => c.Id == request.Id, cancellationToken);

        if (exists)
        {
            return Result.BadRequest("A category with this ID already exists.");
        }

        var category = new Category
        {
            Id = request.Id,
            Description = request.Description,
            CreationTime = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        var categoryModel = _mapper.Map<CategoryModel>(category);
        var result = Result.Created(categoryModel);
        return result;
    }
}
