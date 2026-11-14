using Reference.Application.Features.FabricColor.Shared;
using Reference.Application.Features.FabricColor.Commands.UpdateFabricColor;
using Reference.Api.Endpoints.FabricColors.Shared;

namespace Reference.Api.Endpoints.FabricColors.UpdateFabricColor;

internal static class UpdateFabricColorEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync).WithName("updateFabricColor")
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, FabricColorRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateFabricColorCommand(id, new FabricColorFields(request.Name, request.IsActive,
            request.SortOrder)), cancellationToken)).ToHttpResult();
}

