using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Enums;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Quizzes;

public class GetPreviewsQueryHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<GetPreviewsQuery, Result<PagedList<QuizPreviewModel>>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PagedList<QuizPreviewModel>>> Handle(GetPreviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Quizzes
            .Where(q => q.Status == QuizStatus.Approved || q.Status == QuizStatus.Live);

        if (!string.IsNullOrEmpty(request.OrganizationId))
        {
            query = query.Where(q => q.OrganizationId == request.OrganizationId);
        }

        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            query = query.Where(q => q.CategoryId == request.CategoryId);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = $"%{request.SearchTerm}%";
            query = query.Where(q => q.Title != null && EF.Functions.Like(q.Title, searchTerm));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var mappedItems = _mapper.Map<List<QuizPreviewModel>>(items);
        var pagedList = new PagedList<QuizPreviewModel>
        {
            Items = mappedItems,
            TotalCount = totalItems,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Result.Success(pagedList);
    }
}
