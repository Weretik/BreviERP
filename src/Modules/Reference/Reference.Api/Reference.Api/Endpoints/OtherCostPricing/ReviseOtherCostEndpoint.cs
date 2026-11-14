using Reference.Application.Features.Pricing.Commands.ReviseOtherCost;
using System.Text.Json.Serialization;

namespace Reference.Api.Endpoints.OtherCostPricing;

/// <summary>Appends an approved version of a non-inventory cost; a retry with the same request id returns the same version.</summary>
internal static class ReviseOtherCostEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("reviseOtherCost").Produces<long>()
            .RequireAuthorization("access:pricing-parameters").RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(string code, ReviseOtherCostRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ReviseOtherCostCommand(code, request.ExpectedVersionId, request.Value,
            request.RequestId), cancellationToken)).ToHttpResult();
}

public sealed record ReviseOtherCostRequest(
    [property: JsonRequired] long ExpectedVersionId,
    [property: JsonRequired] decimal Value,
    [property: JsonRequired] Guid RequestId);
