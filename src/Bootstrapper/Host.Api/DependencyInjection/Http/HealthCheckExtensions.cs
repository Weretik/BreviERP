using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Host.Api.DependencyInjection.Http;

/// <summary>
/// `/health` is liveness (the process answers, no dependencies); `/health/ready` also checks PostgreSQL and answers
/// 503 when the database is unreachable (operations: diagnostics).
/// </summary>
public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthEndpoints(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name, tags: [DatabaseHealthCheck.ReadyTag]);
        return services;
    }

    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(DatabaseHealthCheck.ReadyTag),
        }).AllowAnonymous();
        return app;
    }
}
