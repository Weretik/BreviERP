using BuildingBlocks.Api.Errors;
using Crm.Api.Access;
using Crm.Application.Features.Customers.Queries.GetCustomers;

namespace Crm.Api.Endpoints.Customers.GetCrmCustomers;

internal static class GetCrmCustomersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmCustomers")
            .WithSummary("List CRM customers in the owner scope")
            .WithDescription("Admin or Manager, server-owned actor. Customer owner scope enforced; merge is Admin-only. Mutation requestId replays the same payload; revisions protect existing roots.")
            .Produces<CrmCustomerPageResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, bool? active = null, int page = 1, int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var actor = CrmActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        if (page is < 1 or > 1_000_000 || pageSize is < 1 or > 100 || search?.Length > 300) return ApiProblems.BadRequest("Сторінка від 1, розмір сторінки від 1 до 100, пошук до 300 символів.");
        var result = await sender.Send(new GetCustomersQuery(actor, search, active, page, pageSize), cancellationToken);
        return Results.Ok(new CrmCustomerPageResponse(result.Value, result.PagedInfo));
    }
}
