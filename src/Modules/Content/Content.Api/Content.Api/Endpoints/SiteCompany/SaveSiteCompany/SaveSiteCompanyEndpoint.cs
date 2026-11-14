using Content.Application.Features.SiteCompany.Shared;
using Content.Application.Features.SiteCompany.Commands.SaveSiteCompany;

namespace Content.Api.Endpoints.SiteCompany.SaveSiteCompany;

/// <summary>Replaces the facts as a whole: 409 when the revision is stale.</summary>
internal static class SaveSiteCompanyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("saveSiteCompany");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, SaveSiteCompanyRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SaveSiteCompanyCommand(request.Revision, request.Phone, request.Email,
            request.City, request.Street, request.MapQuery, request.Schedule, request.Managers, request.BankDetails,
            request.Socials, ContentEditor.By(user)), cancellationToken)).ToHttpResult();
}
