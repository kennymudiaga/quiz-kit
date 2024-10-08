namespace QuizKit.Common.Models.Quizes;

public record PrizeModel
{
    public string? Id { get; init; }
    public string? QuizId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public decimal? MonetaryValue { get; init; }
    public string? Currency { get; init; }
}
