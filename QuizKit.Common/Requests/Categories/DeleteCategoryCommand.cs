using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Categories;

public record DeleteCategoryCommand : IRequest<Result>
{
    public required string Id { get; init; }
}
