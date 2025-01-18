using System.Text.Json.Serialization;

namespace QuizKit.Common.Models.Users;

public class UserViewModel
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string PhoneNumber { get; init; }
    
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}".Trim();
}
