using Reference.Application.Features.FabricColor.Shared;
using Reference.Application.Features.FabricColor.Commands.CreateFabricColor;
using Reference.Api.Endpoints.FabricColors.Shared;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.FabricColors.CreateFabricColor;

internal static class CreateFabricColorEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFabricColor")
            .WithSummary("Add a colour")
            .WithDescription("Requires access:materials. The ID is assigned by Brevi; a name that differs from another colour only in case or spaces is a duplicate (400).").Produces<int>()
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(FabricColorRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CreateFabricColorCommand(new FabricColorFields(request.Name, request.IsActive,
            request.SortOrder)), cancellationToken)).ToHttpResult();
}
