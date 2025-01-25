using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Questions;

public record CreateQuestionCommand : IRequest<Result<QuestionModel>>
{
    /// <summary>
    /// ID of the quiz this question belongs to
    /// </summary>
    public required string QuizId { get; init; }

    /// <summary>
    /// The question text
    /// </summary>
    public required string QuestionText { get; init; }

    /// <summary>
    /// Option A
    /// </summary>
    public required string A { get; init; }

    /// <summary>
    /// Option B
    /// </summary>
    public required string B { get; init; }

    /// <summary>
    /// Option C
    /// </summary>
    public required string C { get; init; }

    /// <summary>
    /// Option D
    /// </summary>
    public required string D { get; init; }

    /// <summary>
    /// The correct answer (A, B, C, or D)
    /// </summary>
    public required string Answer { get; init; }
}
