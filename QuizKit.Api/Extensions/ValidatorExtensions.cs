using FluentValidation;
using QuizKit.Core.Validators.Users;

namespace QuizKit.Api.Extensions;

public static class ValidatorExtensions
{
    /// <summary>
    /// Extension method to register all FluentValidation validators
    /// </summary>
    /// <param name="services">The service-collection for the application</param>
    /// <returns>The application service collection - to allow for chaining.</returns>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(SignUpCommandValidator).Assembly);

        return services;
    }
}
