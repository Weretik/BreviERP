using Crm.Api.Access;
using Crm.Application.Features.Quotes.Queries.GetQuoteSendDraft;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteSendDraft;

/// <summary>«Надіслати КП»: the sender's Gmail and the letters with marks filled (R-15).</summary>
internal static class GetCrmQuoteSendDraftEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:long}/send-draft", HandleAsync).WithName("getCrmQuoteSendDraft")
            .WithSummary("Get CRM quote send draft")
            .WithDescription("«Надіслати КП» (R-15): the signed-in person's connected Gmail (null until connected), whether Gmail is configured on the server, the PDF file name, and the letters (template from the settings first, then the letter variants) with {quoteNumber}, {managerName}, {managerPhone}, {managerEmail}, {site} filled with the sender's КП contact, {clientName} and {companyName} with the КП «Отримувач» and «Підприємство» (empty when not filled).")
            .Produces<QuoteSendDraftResponse>();

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetQuoteSendDraftQuery(actor, id), cancellationToken)).ToHttpResult();
    }
}
