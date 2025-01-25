using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Questions;

public class GetQuestionQueryHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<GetQuestionQuery, Result<QuestionModel>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<QuestionModel>> Handle(GetQuestionQuery request, CancellationToken cancellationToken)
    {
        var question = await _context.QuizQuestions
            .FirstOrDefaultAsync(q => q.Id == request.Id && q.QuizId == request.QuizId, cancellationToken);

        if (question == null)
        {
            return Result.NotFound();
        }

        var questionModel = _mapper.Map<QuestionModel>(question);
        return Result.Success(questionModel);
    }
}
