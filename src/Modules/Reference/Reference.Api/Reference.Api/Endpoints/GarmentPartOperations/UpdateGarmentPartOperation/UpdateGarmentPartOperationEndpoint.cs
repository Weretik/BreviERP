using Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentPartOperations.UpdateGarmentPartOperation;

internal static class UpdateGarmentPartOperationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync)
            .RequireAuthorization("access:operations");

    private static async Task<IResult> HandleAsync(int id, UpdateGarmentPartOperationRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateGarmentPartOperationCommand(id, new UpdateGarmentPartOperationCommandRequest(
            request.GarmentPartName, request.Name, request.Min, request.Code, request.Element, request.Variant,
            request.MinutesPerProduct, request.IsActive, request.Notes)), cancellationToken)).ToHttpResult();
}

/// <summary>An operation card. Fields after <c>Min</c> come from the Calculator sheet «Операції».</summary>
public sealed record UpdateGarmentPartOperationRequest(
    string GarmentPartName,
    string Name,
    decimal Min,
    string? Code = null,
    string? Element = null,
    string? Variant = null,
    bool MinutesPerProduct = false,
    bool IsActive = true,
    string? Notes = null);
