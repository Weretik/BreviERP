using Crm.Api.Access;
using Crm.Application.Features.Customers.Queries.GetCustomers;

namespace Crm.Api.Endpoints.Customers.GetCrmCustomers;

internal static class GetCrmCustomersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmCustomers");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, bool? active = null, int page = 1, int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var actor = CrmActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        if (page is < 1 or > 1_000_000 || pageSize is < 1 or > 100 || search?.Length > 300) return Results.BadRequest("invalid_pagination");
        var result = await sender.Send(new GetCustomersQuery(actor, search, active, page, pageSize), cancellationToken);
        return Results.Ok(new { value = result.Value, pagedInfo = result.PagedInfo });
    }
}
