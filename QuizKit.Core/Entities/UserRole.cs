namespace QuizKit.Core.Entities;

public record UserRole
{
    protected UserRole() { }
    public UserRole(string userId, string role)
    {
        UserProfileId = userId;
        Role = role.ToLower();
    }

    public string UserProfileId { get; protected set; } = string.Empty;
    public string Role { get; protected set; } = string.Empty;
}
