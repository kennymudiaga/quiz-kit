using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Questions;

public record GetQuestionsQuery : IRequest<Result<List<QuestionModel>>>
{
    /// <summary>
    /// ID of the quiz to get questions for
    /// </summary>
    public required string QuizId { get; init; }
}
