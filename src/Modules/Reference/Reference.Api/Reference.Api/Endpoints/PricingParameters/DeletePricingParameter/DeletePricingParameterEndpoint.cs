using Reference.Application.Features.PricingParameter.Commands.DeletePricingParameter;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PricingParameters.DeletePricingParameter;

internal static class DeletePricingParameterEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeletePricingParameterCommand(id), cancellationToken)).ToHttpResult();
}
