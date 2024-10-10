using MediatR;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;

namespace QuizKit.Core.RequestHandlers.Users;

public class SignUpCommandHandler : IRequestHandler<SignUpCommand, Result<LoggedInUserModel>>
{
    public Task<Result<LoggedInUserModel>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
