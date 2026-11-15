using Crm.Application.Features.Quotes.Queries.CalculateQuoteCustomSewing;
using Crm.Application.Features.Quotes.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.CalculateCrmQuoteCustomSewing;

/// <summary>
/// «Виріб з нуля»: prices each fabric with the chosen accessories and operations by the Catalog formula. Nothing is
/// saved; a fabric that cannot be priced lists its problems.
/// </summary>
internal static class CalculateCrmQuoteCustomSewingEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("custom-sewing-price", HandleAsync).WithName("calculateCrmQuoteCustomSewing")
            .WithSummary("Calculate CRM quote custom sewing")
            .WithDescription("«Виріб з нуля» (R-20, R-26) or a model row in another fabric (R-27): prices each variant, the sum of its fabrics with the shared accessories and operations, by the Catalog sewing formula and the current settings and reference prices. Nothing is saved; a variant that cannot be priced lists its problems.")
            .Produces<QuoteCustomSewingPriceResponse>();

    private static async Task<IResult> HandleAsync(QuoteCustomSewingPriceInput input, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CalculateQuoteCustomSewingQuery(input), cancellationToken)).ToHttpResult();
}
