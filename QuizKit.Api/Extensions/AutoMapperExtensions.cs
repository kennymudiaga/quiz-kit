using QuizKit.Core.Mappers;

namespace QuizKit.Api.Extensions;

public static class AutoMapperExtensions
{
    // Create extension method to register all AutoMapper profiles
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(UserMappingProfile).Assembly);

        return services;
    }
}
