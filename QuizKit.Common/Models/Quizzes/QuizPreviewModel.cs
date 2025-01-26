namespace QuizKit.Common.Models.Quizzes;

public record QuizPreviewModel
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public int? TimeLimit { get; init; }
    public int? QuestionsCount { get; init; }
    public string? Category { get; init; }
    public DateTime? CreatedAt { get; init; }
}
