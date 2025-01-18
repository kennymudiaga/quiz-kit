namespace QuizKit.Core.Entities;

public record Category
{
    /// <summary>
    /// The name of the category. Also serves as the unique identifier of the category. 
    /// </summary>
    public string? Id { get; set; }
    public string? Description { get; set; }

    public DateTime CreationTime { get; init; }
    public DateTime? LastUpdateTime { get; init; }
}
