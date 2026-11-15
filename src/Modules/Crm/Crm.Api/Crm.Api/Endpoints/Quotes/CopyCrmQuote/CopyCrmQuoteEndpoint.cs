using Crm.Application.Features.Quotes.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.CopyQuote;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.CopyCrmQuote;

/// <summary>«Копіювати»: a new draft with the next number and the same items and saved prices.</summary>
internal static class CopyCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/copy", HandleAsync).WithName("copyCrmQuote")
            .WithSummary("«Копіювати»: a new draft with the next number, the same header, introduction and items with their saved prices")
            .WithDescription("«Копіювати»: a new draft with the next number, the same header, introduction and items with their saved prices. The actor becomes its author and responsible.")
            .Produces<QuoteResponse>(StatusCodes.Status201Created);

    private static async Task<IResult> HandleAsync(long id, CopyQuoteInput? input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new CopyQuoteCommand(actor, id, input), cancellationToken);
        return result.IsSuccess
            ? Results.CreatedAtRoute("getCrmQuote", new { id = result.Value.Id }, result.Value)
            : result.ToHttpResult();
    }
}
