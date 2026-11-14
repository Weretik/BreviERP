using Warehouse.Application.Features.Stock.Queries.GetFabricMovements;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Warehouse.Api.Endpoints.FabricStock.GetFabricMovements;

/// <summary>«Рух тканини»: 50 per page, newest operation date first; filters by kind, fabric, colour, item, text and operation-date period, with the count of every kind.</summary>
internal static class GetFabricMovementsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("movements", HandleAsync).WithName("getFabricMovements").Produces<FabricMovementPageResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, int page = 1, QueryEnum<FabricMovementFilter>? kind = null, string? fabricCode = null, string? color = null, int? itemId = null, string? search = null, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricMovementsQuery(actor, page, kind?.Value ?? FabricMovementFilter.All, fabricCode, color, itemId, search, from, to), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
