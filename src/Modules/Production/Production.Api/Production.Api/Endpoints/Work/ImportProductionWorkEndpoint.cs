using Production.Application.Features.Work.Commands.ImportProductionWork;
using Production.Application.Features.Work.Shared;

namespace Production.Api.Endpoints.Work;

/// <summary>Moves the work log of the old «Цех» (Admin); <c>dryRun</c> only shows it.</summary>
internal static class ImportProductionWorkEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("work/import", HandleAsync).WithName("importProductionWork");

    private static async Task<IResult> HandleAsync(ProductionWorkImportInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, bool dryRun = default, CancellationToken cancellationToken = default) =>
        user.Actor() is { } actor
            ? (await sender.Send(new ImportProductionWorkCommand(actor, input.Rows, dryRun),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
