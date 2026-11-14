using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.CopyQuote;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.CopyCrmQuote;

/// <summary>«Копіювати»: a new draft with the next number and the same items and saved prices.</summary>
internal static class CopyCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/copy", HandleAsync).WithName("copyCrmQuote");

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new CopyQuoteCommand(actor, id), cancellationToken);
        return result.IsSuccess
            ? Results.CreatedAtRoute("getCrmQuote", new { id = result.Value.Id }, result.Value)
            : result.ToHttpResult();
    }
}
