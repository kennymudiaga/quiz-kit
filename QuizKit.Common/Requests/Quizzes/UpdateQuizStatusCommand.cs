using MediatR;
using QuizKit.Common.Enums;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizzes;

public record UpdateQuizStatusCommand : IRequest<Result<QuizModel>>
{
    public required string Id { get; set; }
    public required QuizStatus Status { get; set; }
}