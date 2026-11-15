using Production.Application.Features.Work.Shared;
using Production.Application.Features.Work.Commands.CreateProductionWork;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Work.CreateProductionWork;

internal static class CreateProductionWorkEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("orders/{id:int}/items/{itemId:int}/work", HandleAsync).WithName("createProductionWork")
            .WithSummary("Records sewn or cut pieces; beyond the plan only as over-plan with a reason")
            .Produces<ProductionItemWorkResponse>();

    private static async Task<IResult> HandleAsync(int id, int itemId, CreateProductionWorkInput input,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new CreateProductionWorkCommand(actor, id, itemId, input),
                cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
