namespace QuizKit.Common.Models.Quizzes;

public record CategoryModel
{
    public string? Id { get; init; }
    public string? Description { get; init; }
    public DateTime CreationTime { get; init; }
    public DateTime? LastUpdateTime { get; init; }
}
