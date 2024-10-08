namespace QuizKit.Common.Models.Quizes;

public record UserCategoryScoreModel
{
    public string? UserId { get; init; }
    public string? CategoryId { get; init; }
    public decimal Score { get; init; }
}
