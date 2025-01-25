using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizzes;

public class DeleteQuizCommand : IRequest<Result>
{
    /// <summary>
    /// Id of the quiz to delete
    /// </summary>
    public string Id { get; set; } = string.Empty;
}
