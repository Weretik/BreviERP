using Production.Application.Features.FabricNorms.Commands.SaveFabricNorm;
using Production.Application.Features.FabricNorms.Shared;

namespace Production.Api.Endpoints.FabricNorms.SaveFabricNorm;

internal static class SaveFabricNormEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{modelCode}", HandleAsync).WithName("saveFabricNorm");

    private static async Task<IResult> HandleAsync(string modelCode, FabricNormInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SaveFabricNormCommand(modelCode, input, user.UserId()),
            cancellationToken)).ToHttpResult();
}
