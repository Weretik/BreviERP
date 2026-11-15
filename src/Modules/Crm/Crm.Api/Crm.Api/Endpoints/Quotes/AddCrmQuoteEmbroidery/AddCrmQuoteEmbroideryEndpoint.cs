using Crm.Application.Features.Quotes.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.AddEmbroideryToQuote;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.AddCrmQuoteEmbroidery;

/// <summary>«Додати в КП» from «Ціна вишивки»: appends the saved calculation as one item; no version needed.</summary>
internal static class AddCrmQuoteEmbroideryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/embroidery", HandleAsync).WithName("addCrmQuoteEmbroidery")
            .WithSummary("Add CRM quote embroidery")
            .WithDescription("«Додати в КП» from «Ціна вишивки» (EMB-001 R-12): appends a saved embroidery calculation as one `Embroidery` item at the end. Its one row is titled with the saved price category, and that category's unit price stands, shown, in all three tiers; it is never repriced. No version is needed: the call only adds, and raises the version. A Manager adds only to own КП (404 otherwise); an introduction is 409; an unknown calculation or one without a price is 400.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .Produces<QuoteResponse>();

    private static async Task<IResult> HandleAsync(long id, AddQuoteEmbroideryRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new AddEmbroideryToQuoteCommand(actor, id, request.CalculationId), cancellationToken)).ToHttpResult();
    }
}
