namespace QuizKit.Core.Entities;

public record QuizResult
{
    public string? QuizId { get; set; }
    public string? UserId { get; set; }
    public string? QuizName { get; set; }
    public string? UserName { get; set; }
    public decimal Score { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? AnswersCSV { get; set; }
}
