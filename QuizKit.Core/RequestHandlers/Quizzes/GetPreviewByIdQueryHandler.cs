using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Enums;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Quizzes;

public class GetPreviewByIdQueryHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<GetPreviewByIdQuery, Result<QuizPreviewModel>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<QuizPreviewModel>> Handle(GetPreviewByIdQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .Where(q => q.Id == request.QuizId)
            .FirstOrDefaultAsync(cancellationToken);

        if (quiz == null)
        {
            return Result.NotFound();
        }

        // Ensure status is Approved or Live
        if (quiz.Status != QuizStatus.Approved && quiz.Status != QuizStatus.Live)
        {
            return Result.BadRequest("Quiz not available");
        }

        var previewModel = _mapper.Map<QuizPreviewModel>(quiz);
        return Result.Success(previewModel);
    }
}
