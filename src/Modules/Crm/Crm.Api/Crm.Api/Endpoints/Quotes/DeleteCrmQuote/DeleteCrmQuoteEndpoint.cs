using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.DeleteQuote;

namespace Crm.Api.Endpoints.Quotes.DeleteCrmQuote;

/// <summary>«Видалити КП»: the «Видаляти» right of «Права доступу» (the Director by default); a КП with a production order stays (409).</summary>
internal static class DeleteCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:long}", HandleAsync).WithName("deleteCrmQuote")
            .RequireAuthorization(CrmAccess.QuotesDelete);

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        int version = default, CancellationToken cancellationToken = default)
    {
        var actor = QuoteActorClaims.Read(user, canDelete: true);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new DeleteQuoteCommand(actor, id, version), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
