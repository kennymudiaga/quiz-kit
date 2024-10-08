namespace QuizKit.Common.Models.Users;

public record UserOrganizationModel
{
    public string? UserId { get; set; }
    public string? OrganizationId { get; set; }
    public string? Role { get; set; }
}
