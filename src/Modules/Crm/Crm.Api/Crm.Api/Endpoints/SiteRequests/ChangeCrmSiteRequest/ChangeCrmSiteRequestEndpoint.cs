using Crm.Application.Features.SiteRequests.Commands.ChangeSiteRequest;
using Crm.Application.Features.SiteRequests.Shared;

namespace Crm.Api.Endpoints.SiteRequests.ChangeCrmSiteRequest;

/// <summary>«Беру», «Готово», «Не заявка», «Повернути в нові»; 409 when the version is stale or it is taken.</summary>
internal static class ChangeCrmSiteRequestEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/change", HandleAsync).WithName("changeCrmSiteRequest")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, ChangeSiteRequestInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        if (CrmActorClaims.UserId(user) is not { } userId) return Results.Forbid();
        return (await sender.Send(
            new ChangeSiteRequestCommand(userId, user.IsInRole("Admin"), id, input), cancellationToken)).ToHttpResult();
    }
}
