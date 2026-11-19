using Microsoft.AspNetCore.RateLimiting;
using Production.Application.Features.Staff.Commands.ImportWorkshopStaff;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Staff.ImportWorkshopStaff;

/// <summary>Adds workers from a list; with <c>dryRun</c> only shows what would be added.</summary>
internal static class ImportWorkshopStaffEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).WithName("importWorkshopStaff").RequireRateLimiting("HeavyOperation")
            .WithSummary("Add workers from a list (up to 500); a worker with the same active name gets the new position")
            .Produces<WorkshopStaffImportResult>();

    private static async Task<IResult> HandleAsync([FromBody] IReadOnlyList<WorkshopEmployeeImportRow> rows,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = default,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new ImportWorkshopStaffCommand(rows, dryRun, user.UserId()),
            cancellationToken)).ToHttpResult();
}
