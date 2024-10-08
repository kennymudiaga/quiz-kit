namespace QuizKit.Common.Models.Quizes;

public record QuizModel
{
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? CategoryId { get; init; }
    public string? OrganizationId { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? ImageUrl { get; init; }
    public List<QuestionModel>? Questions { get; init; }
}
