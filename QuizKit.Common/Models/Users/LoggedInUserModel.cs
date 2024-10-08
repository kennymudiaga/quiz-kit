namespace QuizKit.Common.Models.Users;

public record LoggedInUserModel
{
    public string? Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Token { get; init; }
    public DateTime? TokenExpiration { get; init; }

    public List<UserOrganizationModel> Organizations { get; init; } = [];
    public List<string> Roles { get; init; } = [];
}
