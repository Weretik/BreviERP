using Scheduling.Api.Endpoints.Tick.RunSchedulerTick;

namespace Scheduling.Api.Endpoints.Tick;

/// <summary>The scheduler's own endpoint, outside <c>/api</c> and the public contract.</summary>
internal static class TickEndpoints
{
    public static IEndpointRouteBuilder MapSchedulerTick(this IEndpointRouteBuilder app)
    {
        RunSchedulerTickEndpoint.Map(app);
        return app;
    }
}
