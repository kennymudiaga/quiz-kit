using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Quizzes;

public class GetQuizzesQueryHandler(QuizDbContext context, IMapper mapper) 
    : IRequestHandler<GetQuizzesQuery, Result<PagedList<QuizModel>>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PagedList<QuizModel>>> Handle(GetQuizzesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Category)
            .Include(q => q.Organization)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.OrganizationId))
        {
            query = query.Where(q => q.OrganizationId == request.OrganizationId);
        }

        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            query = query.Where(q => q.CategoryId == request.CategoryId);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(q => q.Status == request.Status.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var pattern = $"%{request.SearchTerm}%";
            query = query.Where(q => 
                EF.Functions.Like(q.Title!, pattern) || 
                (q.Description != null && EF.Functions.Like(q.Description, pattern)));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var mappedItems = _mapper.Map<List<QuizModel>>(items);

        var pagedList = new PagedList<QuizModel>
        {
            Items = mappedItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalItems
        };

        return Result.Success(pagedList);
    }
}
