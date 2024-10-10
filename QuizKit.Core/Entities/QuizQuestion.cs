namespace QuizKit.Core.Entities;

public record QuizQuestion
{
    public int Id { get; set; }
    public string? QuizId { get; set; }
    public string? QuestionText { get; set; }
    public string? A { get; set; }
    public string? B { get; set; }
    public string? C { get; set; }
    public string? D { get; set; }
    public string? Answer { get; set; }
}
