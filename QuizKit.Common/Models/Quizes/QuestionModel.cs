namespace QuizKit.Common.Models.Quizes;

public record QuestionModel
{
    public int Id { get; init; }
    public string? QuizId { get; init; }
    public string? QuestionText { get; init; }
    public string? A { get; init; }
    public string? B { get; init; }
    public string? C { get; init; }
    public string? D { get; init; }
    public string? Answer { get; init; }
}