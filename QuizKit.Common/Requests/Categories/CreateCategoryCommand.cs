using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Categories;

public record CreateCategoryCommand : IRequest<Result<CategoryModel>>
{
    public required string Id { get; init; }
    public string? Description { get; init; }
}
