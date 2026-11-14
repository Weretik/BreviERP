using BuildingBlocks.Application.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Api.Outbox;

/// <summary>
/// <c>POST /internal/outbox/dispatch</c>: one scheduled run of the outbox (PLT-003 R-06). Cloud Scheduler calls it
/// every minute; it answers at once when nothing is due. Not part of the public contract.
/// </summary>
public static class OutboxDispatchEndpoint
{
    public const string Route = "/internal/outbox/dispatch";

    public static IServiceCollection AddOutboxDispatchEndpoint(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutboxDispatchOptions>(configuration.GetSection(OutboxDispatchOptions.Section));
        services.TryAddSingleton<IOutboxCallerCheck, GoogleOutboxCallerCheck>();
        return services;
    }

    public static IEndpointRouteBuilder MapOutboxDispatch(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(Route, DispatchAsync).AllowAnonymous().ExcludeFromDescription();
        return endpoints;
    }

    private static async Task<IResult> DispatchAsync(HttpRequest request, IOptions<OutboxDispatchOptions> options,
        IOutboxCallerCheck callers, IOutboxDispatcher dispatcher, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.IsConfigured) return Results.NotFound();
        var header = request.Headers.Authorization.ToString();
        const string bearer = "Bearer ";
        if (!header.StartsWith(bearer, StringComparison.OrdinalIgnoreCase)
            || !await callers.IsSchedulerAsync(header[bearer.Length..].Trim(), settings))
            return Results.Unauthorized();

        var run = await dispatcher.DispatchDueAsync(cancellationToken);
        return Results.Ok(new { processed = run.Processed, failed = run.Failed });
    }
}
