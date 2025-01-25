using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizzes;

public class UpdateQuizCommand : IRequest<Result<QuizModel>>
{
    /// <summary>
    /// Id of the quiz to update
    /// </summary>
    public string Id { get; set; } = string.Empty;

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
    /// Id of the organization that owns this quiz
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    /// Whether to randomize the order of questions when taking the quiz
    /// </summary>
    public bool RandomizeQuestions { get; set; }

    /// <summary>
    /// Whether to show answers after submitting the quiz
    /// </summary>
    public bool ShowAnswers { get; set; }

    /// <summary>
    /// Optional category Id for the quiz
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Optional image URL for the quiz
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Optional start date for the quiz
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Optional end date for the quiz
    /// </summary>
    public DateTime? EndDate { get; set; }
}
