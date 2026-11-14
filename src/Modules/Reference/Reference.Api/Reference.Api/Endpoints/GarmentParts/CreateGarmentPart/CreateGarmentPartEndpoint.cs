using Reference.Application.Features.GarmentPart.Commands.CreateGarmentPart;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentParts.CreateGarmentPart;

internal static class CreateGarmentPartEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).Produces<int>();

    private static async Task<IResult> HandleAsync(CreateGarmentPartRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CreateGarmentPartCommand(new CreateGarmentPartCommandRequest(request.Id,
            request.Name)), cancellationToken)).ToHttpResult();
}

public sealed record CreateGarmentPartRequest(int Id, string Name);
