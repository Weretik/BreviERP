using Crm.Application.Features.SiteRequests.Commands.ChangeSiteRequest;
using Crm.Application.Features.SiteRequests.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.SiteRequests.ChangeCrmSiteRequest;

/// <summary>«Беру», «Готово», «Не заявка», «Повернути в нові»; 409 when the version is stale or it is taken.</summary>
internal static class ChangeCrmSiteRequestEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/change", HandleAsync).WithName("changeCrmSiteRequest")
            .WithSummary("Take (anyone with edit), Done or Spam with a note, Reopen (the owner or an Admin)")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<SiteRequestResponse>();

    private static async Task<IResult> HandleAsync(long id, ChangeSiteRequestInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        if (CrmActorClaims.UserId(user) is not { } userId) return Results.Forbid();
        return (await sender.Send(
            new ChangeSiteRequestCommand(userId, user.IsInRole("Admin"), id, input), cancellationToken)).ToHttpResult();
    }
}
