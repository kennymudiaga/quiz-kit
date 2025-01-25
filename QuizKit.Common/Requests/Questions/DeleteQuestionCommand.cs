using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Questions;

public record DeleteQuestionCommand : IRequest<Result>
{
    /// <summary>
    /// ID of the quiz this question belongs to
    /// </summary>
    public required string QuizId { get; init; }

    /// <summary>
    /// ID of the question to delete
    /// </summary>
    public required string Id { get; init; }
}
