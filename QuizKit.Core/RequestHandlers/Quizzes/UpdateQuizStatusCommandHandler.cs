using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuizKit.Common.Enums;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Requests.Quizzes;
using QuizKit.Common.Results;
using QuizKit.Core.Data;

namespace QuizKit.Core.RequestHandlers.Quizzes;

public class UpdateQuizStatusCommandHandler(
    QuizDbContext dbContext,
    IMapper mapper) : IRequestHandler<UpdateQuizStatusCommand, Result<QuizModel>>
{
    public async Task<Result<QuizModel>> Handle(UpdateQuizStatusCommand request, CancellationToken cancellationToken)
    {
        // Find the quiz by Id
        var quiz = await dbContext.Quizzes.FirstOrDefaultAsync(q => q.Id == request.Id);
        if (quiz == null)
        {
            return Result.NotFound();
        }

        if(quiz.Status == QuizStatus.Closed)
        {
            return Result.BadRequest("Quiz is already closed.");
        }
        
        // if start time less than 5 minutes from now
        if (quiz.StartDate < DateTime.UtcNow.AddMinutes(5))
        {
            return Result.BadRequest("Quiz cannot be updated because start time is less than 5 minutes from now.");
        }

        // Update the status
        quiz.Status = request.Status;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<QuizModel>(quiz));
    }
}