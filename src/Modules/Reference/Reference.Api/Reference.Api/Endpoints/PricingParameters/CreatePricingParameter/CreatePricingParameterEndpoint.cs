using Reference.Application.Features.PricingParameter.Commands.CreatePricingParameter;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PricingParameters.CreatePricingParameter;

internal static class CreatePricingParameterEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).Produces<int>()
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(CreatePricingParameterRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreatePricingParameterCommand(new CreatePricingParameterCommandRequest(request.Code,
            request.Group,
            request.Name, request.Value, request.Unit, request.IsCalculated, request.Comment, request.SortOrder)),
                cancellationToken)).ToHttpResult();
}

/// <summary>A new parameter of «Налаштування цін»; percentages are fractions (0.25 = 25 %).</summary>
public sealed record CreatePricingParameterRequest(
    string Code,
    string Group,
    string Name,
    decimal? Value,
    string Unit,
    bool IsCalculated = false,
    string? Comment = null,
    int SortOrder = 0);
