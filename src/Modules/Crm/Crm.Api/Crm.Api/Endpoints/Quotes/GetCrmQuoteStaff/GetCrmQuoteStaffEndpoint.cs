using Crm.Application.Features.Quotes.Queries.GetQuoteStaff;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteStaff;

internal static class GetCrmQuoteStaffEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("staff", HandleAsync).WithName("getCrmQuoteStaff")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteStaffQuery(), cancellationToken)).ToHttpResult();
}
