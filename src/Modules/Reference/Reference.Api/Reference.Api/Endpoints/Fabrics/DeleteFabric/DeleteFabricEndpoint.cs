using Reference.Application.Features.Fabric.Commands.DeleteFabric;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Fabrics.DeleteFabric;

internal static class DeleteFabricEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .WithName("deleteFabric")
            .WithSummary("Delete a fabric")
            .WithDescription("Requires access:materials. Numeric edits append an approved pricing revision when the reference is versioned.")
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteFabricCommand(id), cancellationToken)).ToHttpResult();
}
