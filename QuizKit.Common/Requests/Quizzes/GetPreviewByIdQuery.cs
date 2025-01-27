using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizzes;

public record GetPreviewByIdQuery : IRequest<Result<QuizPreviewModel>>
{
    public required string QuizId { get; init; }
}
