using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizzes;

public class CreateQuizCommand : IRequest<Result<QuizModel>>
{
    /// <summary>
    /// Title of the quiz
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description of the quiz
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Time limit in minutes for the quiz. If null, there is no time limit.
    /// </summary>
    public int? TimeLimit { get; set; }

    /// <summary>
    /// Whether to randomize the order of questions
    /// </summary>
    public bool RandomizeQuestions { get; set; }

    /// <summary>
    /// Whether to show the correct answers after submission
    /// </summary>
    public bool ShowAnswers { get; set; }

    /// <summary>
    /// The organization ID that this quiz belongs to
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    /// The category ID that this quiz belongs to
    /// </summary>
    public string? CategoryId { get; set; }
}
