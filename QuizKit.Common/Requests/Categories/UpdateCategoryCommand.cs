using MediatR;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Categories;

public record UpdateCategoryCommand : IRequest<Result<CategoryModel>>
{
    public string? Id { get; init; }
    public string? Description { get; init; }
}
