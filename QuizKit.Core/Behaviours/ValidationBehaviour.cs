using FluentValidation;
using MediatR;
using QuizKit.Common.Results;

namespace QuizKit.Core.Behaviours;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result, new()
{
    private readonly IEnumerable<IValidator<TRequest>> validators = validators?.ToList() ?? [];

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, List<string>>();
        foreach (var validator in validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            var data = validationResult.ToDictionary();
            foreach (var item in data)
            {
                if (errors.TryGetValue(item.Key, out List<string>? value))
                {
                    value.AddRange(item.Value);
                }
                else
                {
                    errors[item.Key] = new List<string>(item.Value);
                }
            }
        }

        if (errors.Count > 0)
        {
            return new TResponse { Errors = errors, Message = Failure.DefaultValidationMessage, Status = ResultStatus.BadRequest };
        }

        return await next();
    }
}

