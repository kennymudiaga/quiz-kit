using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizzes;

public record GetQuizQuery : IRequest<Result<QuizModel>>
{
    /// <summary>
    /// Id of the quiz to retrieve
    /// </summary>
    public required string Id { get; init; }
}
