using Reference.Application.Features.GarmentPartOperation.Commands.CreateGarmentPartOperation;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentPartOperations.CreateGarmentPartOperation;

internal static class CreateGarmentPartOperationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync)
            .WithName("createGarmentPartOperation")
            .WithSummary("Create a garment part operation with a client supplied ID")
            .WithDescription("Requires access:operations. The code (OP-…) is unique when given; a duplicate is a 400 on Request.Code. Numeric edits append an approved pricing revision when the reference is versioned.")
            .Produces<int>()
            .RequireAuthorization("access:operations");

    private static async Task<IResult> HandleAsync(CreateGarmentPartOperationRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateGarmentPartOperationCommand(new CreateGarmentPartOperationCommandRequest(request.Id,
            request.GarmentPartName, request.Name, request.Min, request.Code, request.Element, request.Variant,
            request.MinutesPerProduct, request.IsActive, request.Notes)), cancellationToken)).ToHttpResult();
}

/// <summary>An operation card. Fields after <c>Min</c> come from the Calculator sheet «Операції».</summary>
public sealed record CreateGarmentPartOperationRequest(
    int Id,
    string GarmentPartName,
    string Name,
    decimal Min,
    string? Code = null,
    string? Element = null,
    string? Variant = null,
    bool MinutesPerProduct = false,
    bool IsActive = true,
    string? Notes = null);
