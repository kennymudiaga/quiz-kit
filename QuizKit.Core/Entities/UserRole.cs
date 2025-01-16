namespace QuizKit.Core.Entities;

public record UserRole
{
    protected UserRole() { }
    public UserRole(string userId, string role)
        : this()
    {
        UserProfileId = userId;
        Role = role.ToLower();
    }

    public string UserProfileId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
