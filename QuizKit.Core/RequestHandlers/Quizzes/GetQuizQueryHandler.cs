using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Quizzes;

public class GetQuizQueryHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<GetQuizQuery, Result<QuizModel>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<QuizModel>> Handle(GetQuizQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Organization)
            .Include(q => q.Category)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (quiz == null)
        {
            return Result.NotFound("Quiz not found.");
        }

        var quizModel = _mapper.Map<QuizModel>(quiz);
        return Result.Success(quizModel);
    }
}
