namespace QuizKit.Core.Entities;

public record UserOrganization
{
    public string? UserProfileId { get; set; }
    public string? OrganizationId { get; set; }
    public DateTime JoinDate { get; set; }
    public string? Role { get; set; }
}
