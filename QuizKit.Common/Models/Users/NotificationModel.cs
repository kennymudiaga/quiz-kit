namespace QuizKit.Common.Models.Users;

public record NotificationModel
{
    public string? Id { get; init; }
    public string? UserId { get; init; }
    public string? Message { get; init; }
    public string? ActionUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public bool IsRead => ReadAt.HasValue;
}
