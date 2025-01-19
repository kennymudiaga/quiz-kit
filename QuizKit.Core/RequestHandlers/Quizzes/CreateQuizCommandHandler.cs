using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Entities;

namespace QuizKit.Core.RequestHandlers.Quizzes;

public class CreateQuizCommandHandler(QuizDbContext context) : IRequestHandler<CreateQuizCommand, Result<QuizModel>>
{
    private readonly QuizDbContext _context = context;

    public async Task<Result<QuizModel>> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
    {
        if(!string.IsNullOrEmpty(request.OrganizationId))
        {
            // Verify organization exists
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.Id == request.OrganizationId, cancellationToken);

            if (!organizationExists)
            {
                return Result.BadRequest("Organization not found.");
            }
        }
        

        var quiz = new Quiz(request);
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync(cancellationToken);

        var quizModel = new QuizModel
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            OrganizationId = quiz.OrganizationId,
            TimeLimit = quiz.TimeLimit,
            RandomizeQuestions = quiz.RandomizeQuestions,
            ShowAnswers = quiz.ShowAnswers,
            CreatedAt = quiz.CreatedAt,
            Questions = new List<QuestionModel>()
        };

        return Result.Success(quizModel);
    }
}
