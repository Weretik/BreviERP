using Crm.Application.Features.Embroidery.Commands.SetEmbroideryMockup;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Api.Endpoints.Embroidery;

/// <summary>«Макет»: a Media library photo of the design for the workshop; null clears it.</summary>
internal static class SetCrmEmbroideryMockupEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("calculations/{id:long}/mockup", HandleAsync).WithName("setCrmEmbroideryMockup");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, long id, SetEmbroideryMockupInput input,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = EmbroideryActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new SetEmbroideryMockupCommand(actor, id, input),
            cancellationToken)).ToHttpResult();
    }
}
