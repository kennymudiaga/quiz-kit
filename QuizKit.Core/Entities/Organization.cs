using QuizKit.Common.Enums;

namespace QuizKit.Core.Entities;

public record Organization : IdEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public EntityStatus? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedById { get; set; }
}
