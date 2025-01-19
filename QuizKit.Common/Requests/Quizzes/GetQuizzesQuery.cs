using MediatR;
using QuizKit.Common.Models;
using QuizKit.Common.Models.Quizzes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizzes
{
    public record GetQuizzesQuery : IRequest<Result<PagedList<QuizModel>>>
    {
        public string? OrganizationId { get; set; }
        public string? CategoryId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
