using MediatR;
using QuizKit.Common.Models.Quizes;
using QuizKit.Common.Results;

namespace QuizKit.Common.Requests.Quizes;

public class CreateQuizCommand : IRequest<Result<QuizModel>>
{
}
