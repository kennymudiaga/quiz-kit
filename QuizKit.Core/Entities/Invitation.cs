namespace QuizKit.Core.Entities;

public record Invitation : IdEntity
{
    public string? Email { get; set; }
    public string? OrganizationId { get; set; }
    public string? TokenHash { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? SenderId { get; set; }
}
