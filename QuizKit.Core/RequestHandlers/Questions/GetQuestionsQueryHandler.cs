using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Questions;

public class GetQuestionsQueryHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<GetQuestionsQuery, Result<List<QuestionModel>>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<List<QuestionModel>>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken);

        if (quiz == null)
        {
            return Result.NotFound();
        }

        var questions = await _context.QuizQuestions
            .Where(q => q.QuizId == request.QuizId)
            .ToListAsync(cancellationToken);

        var questionModels = _mapper.Map<List<QuestionModel>>(questions);
        return Result.Success(questionModels);
    }
}
