using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Quizzes;

public class UpdateQuizCommandHandler(QuizDbContext context) : IRequestHandler<UpdateQuizCommand, Result<QuizModel>>
{
    private readonly QuizDbContext _context = context;

    public async Task<Result<QuizModel>> Handle(UpdateQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (quiz == null)
        {
            return Result.NotFound("Quiz not found.");
        }

        if (!string.IsNullOrEmpty(request.OrganizationId) && request.OrganizationId != quiz.OrganizationId)
        {
            // Verify new organization exists if it's being changed
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.Id == request.OrganizationId, cancellationToken);

            if (!organizationExists)
            {
                return Result.BadRequest("Organization not found.");
            }
        }

        if (!string.IsNullOrEmpty(request.CategoryId) && request.CategoryId != quiz.CategoryId)
        {
            // Verify new category exists if it's being changed
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (!categoryExists)
            {
                return Result.BadRequest("Category not found.");
            }
        }

        quiz.Update(request);
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
            CategoryId = quiz.CategoryId,
            ImageUrl = quiz.ImageUrl,
            StartDate = quiz.StartDate,
            EndDate = quiz.EndDate,
            CreatedAt = quiz.CreatedAt
        };

        return Result.Success(quizModel);
    }
}
