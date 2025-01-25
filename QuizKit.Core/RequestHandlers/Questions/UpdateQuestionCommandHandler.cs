using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Questions;

public class UpdateQuestionCommandHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<UpdateQuestionCommand, Result<QuestionModel>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<QuestionModel>> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.QuizQuestions
            .FirstOrDefaultAsync(q => q.Id == request.Id && q.QuizId == request.QuizId, cancellationToken);

        if (question == null)
        {
            return Result.NotFound();
        }

        question.QuestionText = request.QuestionText;
        question.A = request.A;
        question.B = request.B;
        question.C = request.C;
        question.D = request.D;
        question.Answer = request.Answer;

        await _context.SaveChangesAsync(cancellationToken);

        var questionModel = _mapper.Map<QuestionModel>(question);
        return Result.Success(questionModel);
    }
}
