using Crm.Api.Quotes;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.AddEmbroideryToQuote;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>«Додати в КП» from «Ціна вишивки»: appends the saved calculation as one item; no version needed.</summary>
internal static class AddCrmQuoteEmbroideryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/embroidery", HandleAsync).WithName("addCrmQuoteEmbroidery")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, AddQuoteEmbroideryRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new AddEmbroideryToQuoteCommand(actor, id, request.CalculationId), cancellationToken)).ToHttpResult();
    }
}
