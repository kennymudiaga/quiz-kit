namespace QuizKit.Common.Models.Quizes;

public record UserScoreModel
{
    public string? Id { get; init; }
    public string? UserId { get; init; }
    public string? DisplayName { get; init; }
    public decimal Score { get; init; }
    public DateTime? LastUpdateTime { get; init; }
}
