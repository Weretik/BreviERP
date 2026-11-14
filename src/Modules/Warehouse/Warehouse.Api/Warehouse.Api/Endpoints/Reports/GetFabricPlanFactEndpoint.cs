using Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.Reports;

/// <summary>«План і факт»: main fabric by model; filters match a part of the model, client or fabric; the period keeps the items whose last issue is in it.</summary>
internal static class GetFabricPlanFactEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("plan-fact", HandleAsync).WithName("getFabricPlanFact").Produces<FabricPlanFactResponse>()
            .RequireAuthorization(WarehouseAccess.FabricPlanFact);

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, string? model = null, string? client = null, string? fabric = null, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricPlanFactQuery(actor, model, client, fabric, from, to), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
