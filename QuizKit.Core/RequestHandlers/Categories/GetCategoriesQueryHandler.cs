using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Categories;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Categories;

public class GetCategoriesQueryHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<GetCategoriesQuery, Result<PagedList<CategoryModel>>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PagedList<CategoryModel>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var pattern = $"%{request.SearchTerm}%";
            query = query.Where(c => 
                EF.Functions.Like(c.Id, pattern) || 
                (c.Description != null && EF.Functions.Like(c.Description, pattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var categoryModels = _mapper.Map<List<CategoryModel>>(items);
        
        var pagedList = new PagedList<CategoryModel>
        {
            Items = categoryModels,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result.Success(pagedList);
    }
}
