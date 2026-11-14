using Production.Api.Access;
using Production.Api.Endpoints.Archive.GetWorkshopArchive;
using Production.Api.Endpoints.Archive.ImportWorkshopArchive;

namespace Production.Api.Endpoints.Archive;

/// <summary>«Архів цеху» (PRD-D54): read-only lines of the old «Цех» for everyone with production orders.</summary>
internal static class WorkshopArchiveEndpoints
{
    public static IEndpointRouteBuilder MapWorkshopArchiveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/archive").RequireAuthorization(ProductionAccess.Archive)
            .WithTags("WorkshopArchive");
        GetWorkshopArchiveEndpoint.Map(group);
        ImportWorkshopArchiveEndpoint.Map(group);
        return app;
    }
}
