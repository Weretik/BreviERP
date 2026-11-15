using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Queries.GetQuoteStaff;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteStaff;

internal static class GetCrmQuoteStaffEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("staff", HandleAsync).WithName("getCrmQuoteStaff")
            .WithSummary("Active Manager and Admin users a КП can be given to")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<IReadOnlyList<QuoteStaffResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteStaffQuery(), cancellationToken)).ToHttpResult();
}
