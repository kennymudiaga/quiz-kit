namespace QuizKit.Core.Entities;

public record UserOrganization
{
    protected UserOrganization()
    {
    }

    public UserOrganization(string userProfileId, string organizationId, string role, DateTime? joinDate = null)
    {
        UserProfileId = userProfileId;
        OrganizationId = organizationId;
        JoinDate = joinDate ?? DateTime.UtcNow;
        Role = role;
    }

    public string? UserProfileId { get; set; }
    public string? OrganizationId { get; set; }
    public DateTime JoinDate { get; set; }
    public string? Role { get; set; }
}
