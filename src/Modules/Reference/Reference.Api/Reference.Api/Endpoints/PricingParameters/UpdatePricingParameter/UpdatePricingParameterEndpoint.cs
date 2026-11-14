using Reference.Application.Features.PricingParameter.Commands.UpdatePricingParameter;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PricingParameters.UpdatePricingParameter;

internal static class UpdatePricingParameterEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync)
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(int id, UpdatePricingParameterRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdatePricingParameterCommand(id,
            new UpdatePricingParameterCommandRequest(request.Group, request.Name,
            request.Value, request.Unit, request.IsCalculated, request.Comment, request.SortOrder)),
                cancellationToken)).ToHttpResult();
}

/// <summary>Editable fields of a parameter; the code cannot change.</summary>
public sealed record UpdatePricingParameterRequest(
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated = false,
    string? Comment = null,
    int SortOrder = 0);
