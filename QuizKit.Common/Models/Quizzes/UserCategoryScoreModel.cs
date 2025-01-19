namespace QuizKit.Common.Models.Quizzes;

public record UserCategoryScoreModel
{
    public string? UserId { get; init; }
    public string? CategoryId { get; init; }
    public decimal Score { get; init; }
}
