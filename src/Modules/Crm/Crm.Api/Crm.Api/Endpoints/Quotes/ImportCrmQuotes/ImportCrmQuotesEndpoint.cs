using Microsoft.AspNetCore.RateLimiting;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.ImportQuotes;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.ImportCrmQuotes;

internal static class ImportCrmQuotesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("importCrmQuotes").RequireRateLimiting("HeavyOperation")
            .WithSummary("One-time transfer of КП from the old calculator (R-25)")
            .WithDescription("One-time transfer of КП from the old calculator (R-25). dryRun=true only returns the report. Otherwise every new КП is saved in one transaction with its calculator number, people, dates, status, prices and history plus an «Imported» line. A КП transferred before is skipped, so the same file can be sent again; a number a new Brevi КП already has gets the next free number of that month. Items are linked to the Catalog and Reference by code; what cannot be linked is transferred as it is and named in warnings.")
            .Produces<QuoteImportReport>();

    private static async Task<IResult> HandleAsync(ImportQuotesRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new ImportQuotesCommand(actor, request.DryRun, request.Quotes),
            cancellationToken)).ToHttpResult();
    }
}
