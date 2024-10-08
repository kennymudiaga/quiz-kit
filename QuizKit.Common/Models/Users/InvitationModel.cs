namespace QuizKit.Common.Models.Users;

public record InvitationModel
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? OrganizationId { get; set; }
    public string? SenderId { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
