using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Questions;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;

namespace QuizKit.Core.RequestHandlers.Questions;

public class CreateQuestionCommandHandler(QuizDbContext context, IMapper mapper) : IRequestHandler<CreateQuestionCommand, Result<QuestionModel>>
{
    private readonly QuizDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<QuestionModel>> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken);

        if (quiz == null)
        {
            return Result.NotFound("Quiz not found.");
        }

        var question = new QuizQuestion
        {
            QuizId = request.QuizId,
            QuestionText = request.QuestionText,
            A = request.A,
            B = request.B,
            C = request.C,
            D = request.D,
            Answer = request.Answer
        };

        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        var questionModel = _mapper.Map<QuestionModel>(question);
        return Result.Success(questionModel);
    }
}
