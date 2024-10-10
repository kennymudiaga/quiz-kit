namespace QuizKit.Core.Entities;

public record Category
{
    public string? Id { get; set; }
    public string? Description { get; set; }

    public DateTime CreationTime { get; init; }
    public DateTime? LastUpdateTime { get; init; }
}
