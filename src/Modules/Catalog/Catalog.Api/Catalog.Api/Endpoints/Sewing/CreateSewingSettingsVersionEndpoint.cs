using Catalog.Api.Pricing.Sewing.Settings;
using Catalog.Application.Features.Pricing.Commands.CreateSewingSettingsVersion;

namespace Catalog.Api.Endpoints.Sewing;

internal static class CreateSewingSettingsVersionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("versions", HandleAsync).WithName("createSewingSettingsVersion")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(CreateSewingSettingsVersionRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Results.Forbid();
        if (request.Settings is null)
            return Results.BadRequest();
        var result = await sender.Send(new CreateSewingSettingsVersionCommand(
            request.ExpectedVersionId, request.RequestId, request.Settings.ToSettings(), actor), cancellationToken);
        return result.ToHttpResult();
    }
}
