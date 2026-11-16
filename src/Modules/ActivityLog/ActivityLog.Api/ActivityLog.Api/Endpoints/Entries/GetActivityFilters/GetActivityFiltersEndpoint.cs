using ActivityLog.Application.Features.Entries.Queries.GetActivityFilters;

namespace ActivityLog.Api.Endpoints.Entries.GetActivityFilters;

internal static class GetActivityFiltersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("filters", HandleAsync).WithName("getActivityFilters")
            .WithSummary("The choices of the «Журнал дій» filters")
            .WithDescription("People and object types found in the journal, and every action.")
            .Produces<ActivityFiltersDto>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetActivityFiltersQuery(), cancellationToken)).ToHttpResult();
}
