using MediatR;
using QuizKit.Core.Behaviours;
using QuizKit.Core.RequestHandlers.Users;

namespace QuizKit.Api.Extensions;

public static class MediatorExtensions
{
    // Create extension method to register MediatR and pipeline behaviors
    public static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        // Add mediator and register handlers
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(typeof(SignUpCommandHandler).Assembly);
            config.Lifetime = ServiceLifetime.Scoped;
        });

        // Add pipeline behaviors - order is important
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


        return services;
    }
}
