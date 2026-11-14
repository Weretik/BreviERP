using Crm.Api.Access;
using Crm.Application.Features.Quotes.Queries.GetQuotes;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuotes;

internal static class GetCrmQuotesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmQuotes");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, string? status = null, int? responsibleUserId = null, int page = 1, int pageSize = 50,
        long? customerId = null, CancellationToken cancellationToken = default)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new GetQuotesQuery(actor, search, status, responsibleUserId, page, pageSize, customerId),
            cancellationToken)).ToHttpResult();
    }
}
