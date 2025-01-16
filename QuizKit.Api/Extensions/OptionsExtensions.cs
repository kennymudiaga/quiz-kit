using QuizKit.Core.Options;
using System.Reflection;

namespace QuizKit.Api.Extensions
{
    public static class OptionsExtensions
    {
        public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
        {
            var options = assembly.GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(OptionsBase))).ToList();
            foreach (var optionType in options)
            {
                var attribute = optionType.GetCustomAttribute<ConfigSectionNameAttribute>();
                string configName = attribute is null ? optionType.Name : attribute.Name;
                object? optionsInstance = configuration.GetSection(configName).Get(optionType);
                if (optionsInstance is not null)
                {
                    services.AddSingleton(optionType, optionsInstance);
                }
            }

            return services;
        }

        public static IServiceCollection ConfigureOptions(this IServiceCollection services,
            IConfiguration configuration, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
                ConfigureOptions(services, configuration, assembly);

            return services;
        }
    }
}
