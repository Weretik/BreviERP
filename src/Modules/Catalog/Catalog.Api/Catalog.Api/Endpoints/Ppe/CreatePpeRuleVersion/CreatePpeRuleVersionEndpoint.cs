using Catalog.Application.Features.Pricing.Commands.CreatePpeRuleVersion;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.CreatePpeRuleVersion;

internal static class CreatePpeRuleVersionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("versions", HandleAsync).WithName("createPpeRuleVersion")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(CreatePpeRuleVersionRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(new CreatePpeRuleVersionCommand(
            request.ExpectedVersionId, request.RequestId, request.Values.ToValues(), actor),
                cancellationToken)).ToHttpResult();
    }
}
