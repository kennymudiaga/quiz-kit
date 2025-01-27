using MediatR;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizzes;

public record GetPreviewsQuery : PagedQuery, IRequest<Result<PagedList<QuizPreviewModel>>>
{
    public string? OrganizationId { get; init; }
    public string? CategoryId { get; init; }
    public string? SearchTerm { get; init; }
}
