
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using JwtFactory;
using QuizKit.Api.Extensions;
using QuizKit.Common.Constants;

namespace QuizKit.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add a local override json file for developer/environment-centric settings
        var overrideEnv = Environment.GetEnvironmentVariable("ENVIRONMENT_OVERRIDE");
        if (!string.IsNullOrWhiteSpace(overrideEnv))
        {
            builder.Configuration.AddJsonFile($"appsettings.{overrideEnv}.json", optional: true);
        }

        // Add services to the container.
        builder.Services.AddAutoMapperProfiles();
        // Add validators - uses FluentValidation
        builder.Services.AddValidators();
        // Add mediator - this will also add the pipeline behaviors
        builder.Services.AddMediatR();

        builder.Services.AddControllers();
        builder.Services.AddJwtProvider(builder.Configuration.GetSection("JWT").Get<JwtInfo>());
        builder.Services.AddAuthorizationBuilder()
            .AddDefaultPolicy(Policies.Default, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(Policies.Admin, policy => policy.RequireRole(Roles.Admin))
            .AddPolicy(Policies.User, policy => policy.RequireRole(Roles.User));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        // Add API versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
        })
        .AddApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VV";
            setup.DefaultApiVersion = new ApiVersion(1, 0);
            setup.AssumeDefaultVersionWhenUnspecified = true;
        });
        builder.Services.AddSwaggerGen();
        builder.Services.ConfigureOptions<SwaggerOptionsConfiguration>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });

        //Use https redirection, except in Docker
        //TODO: resolve certificate issues in Docker
        //if (overrideEnv != "Docker")
        //{
        //    app.UseHttpsRedirection();
        //}
        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
