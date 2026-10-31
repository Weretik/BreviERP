using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Host.Api.Security.AnonymousApiAccess;

public static class AnonymousApiAccessExtensions
{
    public static IServiceCollection AddAnonymousApiAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AnonymousApiAccessOptions>()
            .Bind(configuration.GetSection(AnonymousApiAccessOptions.SectionName))
            .Validate(options => options.ActorUserId is null or > 0, "ActorUserId must be positive when provided.")
            .ValidateOnStart();
        services.AddScoped<IAnonymousApiActorResolver, IdentityAnonymousApiActorResolver>();
        if (configuration.GetValue<bool>("ApiAccess:AnonymousEnabled"))
            services.Configure<SwaggerGenOptions>(options => options.SwaggerGeneratorOptions.SecurityRequirements.Clear());
        return services;
    }

    public static IApplicationBuilder UseAnonymousApiAccess(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetRequiredService<IOptions<AnonymousApiAccessOptions>>().Value.AnonymousEnabled)
            app.UseMiddleware<AnonymousApiAccessMiddleware>();
        return app;
    }

    public static ControllerActionEndpointConventionBuilder MapApiControllers(this WebApplication app)
    {
        var controllers = app.MapControllers();
        if (app.Services.GetRequiredService<IOptions<AnonymousApiAccessOptions>>().Value.AnonymousEnabled)
            controllers.AllowAnonymous();
        return controllers;
    }

    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() ||
            app.Services.GetRequiredService<IOptions<AnonymousApiAccessOptions>>().Value.AnonymousEnabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOpenApi().AllowAnonymous();
        }
        return app;
    }
}
