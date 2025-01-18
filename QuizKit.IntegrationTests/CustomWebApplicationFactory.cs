using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizKit.Api;
using QuizKit.Common.Models.Users;
using QuizKit.Common.Requests.Users;
using QuizKit.Common.Results;
using QuizKit.Core.Data;
using QuizKit.Core.Options;
using QuizKit.Core.ServiceContracts;

namespace QuizKit.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the application's DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<QuizDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using an in-memory database for testing
            services.AddDbContext<QuizDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Replace the ITokenGenerator service with a mock
            var descriptorTokenGenerator = services.SingleOrDefault(
                d => d.ServiceType == typeof(ITokenGenerator));
            if (descriptorTokenGenerator != null)
            {
                services.Remove(descriptorTokenGenerator);
            }
            services.AddScoped<ITokenGenerator, MockTokenGenerator>();

            // Add custom user policy options
            services.AddSingleton(new UserPolicyOptions
            {
                EnableLockout = true,
                MaxPasswordFailCount = 3,
                PasswordLockoutDuration = 5,
                PasswordTokenTimeout = 0.33d, // 20 seconds
            });

            // Add a Test Login Pipeline Behavior
            services.AddScoped(typeof(IPipelineBehavior<LoginCommand, Result<LoggedInUserModel>>), typeof(LoginTestUserBehavior));

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<QuizDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });

        // Optional: Override configuration to use test-specific settings
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "DataSource=:memory:"},
                {"Environment", "Testing"},
            });
        });
    }
}
