using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Questions;

public record GetQuestionQuery : IRequest<Result<QuestionModel>>
{
    /// <summary>
    /// ID of the quiz this question belongs to
    /// </summary>
    public required string QuizId { get; init; }

    /// <summary>
    /// ID of the question to get
    /// </summary>
    public required string Id { get; init; }
}
