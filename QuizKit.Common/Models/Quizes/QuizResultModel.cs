namespace QuizKit.Common.Models.Quizes;

public record QuizResultModel
{
    public string? UserId { get; init; }
    public string? QuizId { get; init; }

    public string? UserName { get; init; }
    public string? QuizName { get; init; }

    /// <summary>
    /// Points from correct answers and time bonus
    /// </summary>
    public decimal Points { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Score from correct answers
    /// </summary>
    public int QuizScore => (int)Math.Floor(Points);
    public decimal TimeBonus => Points - QuizScore;
    public TimeSpan CompletionTime => EndTime - StartTime;
}
