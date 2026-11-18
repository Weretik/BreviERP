using Microsoft.AspNetCore.RateLimiting;
using Production.Application.Features.Work.Commands.ImportProductionWork;
using Production.Application.Features.Work.Shared;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Work.ImportProductionWork;

/// <summary>Moves the work log of the old «Цех» (Admin); <c>dryRun</c> only shows it.</summary>
internal static class ImportProductionWorkEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("work/import", HandleAsync).WithName("importProductionWork").RequireRateLimiting("HeavyOperation")
            .WithSummary("Moves the work log of the old «Цех» (Admin); nothing is saved unless every row is fine")
            .Produces<ProductionWorkImportResult>();

    private static async Task<IResult> HandleAsync(ProductionWorkImportInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, bool dryRun = default, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ImportProductionWorkCommand(actor, input.Rows, dryRun),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
