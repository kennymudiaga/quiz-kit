
namespace QuizKit.Core.Entities;

public record Quiz : IdEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public string? OrganizationId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ImageUrl { get; set; }

    protected readonly List<QuizQuestion> _questions = [];

    public IReadOnlyList<QuizQuestion> Questions => _questions;

    public virtual Category? Category { get; set; }
    public virtual Organization? Organization { get; set; }
}
