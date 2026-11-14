using Reference.Application.Features.GarmentPart.Commands.UpdateGarmentPart;

namespace Reference.Api.Endpoints.GarmentParts.UpdateGarmentPart;

internal static class UpdateGarmentPartEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, UpdateGarmentPartRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateGarmentPartCommand(id, new UpdateGarmentPartCommandRequest(request.Name)),
            cancellationToken)).ToHttpResult();
}

public sealed record UpdateGarmentPartRequest(string Name);
