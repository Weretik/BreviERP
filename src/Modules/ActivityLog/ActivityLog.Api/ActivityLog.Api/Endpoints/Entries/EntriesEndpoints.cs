using ActivityLog.Api.Endpoints.Entries.GetActivityEntries;
using ActivityLog.Api.Endpoints.Entries.GetActivityFilters;

namespace ActivityLog.Api.Endpoints.Entries;

/// <summary>«Журнал дій»: the entries and the choices of their filters. No writes: entries come from the outbox.</summary>
internal static class EntriesEndpoints
{
    public static IEndpointRouteBuilder MapActivityEntriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/activity-log").WithTags("ActivityLog")
            .RequireAuthorization(ActivityLogAccess.Page);
        GetActivityEntriesEndpoint.Map(group);
        GetActivityFiltersEndpoint.Map(group);
        return app;
    }
}
