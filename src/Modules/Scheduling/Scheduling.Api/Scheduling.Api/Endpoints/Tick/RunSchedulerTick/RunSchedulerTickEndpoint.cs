using BuildingBlocks.Api.Outbox;
using Microsoft.Extensions.Options;
using Scheduling.Application.Features.Tick.Commands.RunSchedulerTick;

namespace Scheduling.Api.Endpoints.Tick.RunSchedulerTick;

/// <summary>
/// <c>POST /internal/scheduler/tick</c>: one minute of the scheduler (SCH-001 R-04, R-10). Not part of the public
/// contract. The caller check is the outbox endpoint's Google OIDC check with the scheduler's audience and account.
/// </summary>
internal static class RunSchedulerTickEndpoint
{
    public const string Route = "/internal/scheduler/tick";

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost(Route, HandleAsync).AllowAnonymous().ExcludeFromDescription();

    private static async Task<IResult> HandleAsync(HttpRequest request, [FromServices] IOptions<SchedulerTickOptions> options,
        [FromServices] IOutboxCallerCheck callers, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.IsConfigured) return Results.NotFound();
        var header = request.Headers.Authorization.ToString();
        const string bearer = "Bearer ";
        var caller = new OutboxDispatchOptions { Audience = settings.Audience, SchedulerServiceAccount = settings.SchedulerServiceAccount };
        if (!header.StartsWith(bearer, StringComparison.OrdinalIgnoreCase)
            || !await callers.IsSchedulerAsync(header[bearer.Length..].Trim(), caller))
            return Results.Unauthorized();

        return (await sender.Send(new RunSchedulerTickCommand(), cancellationToken)).ToHttpResult();
    }
}
