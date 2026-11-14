using Crm.Application.Features.Quotes.Queries.CalculateQuoteCustomSewing;
using Crm.Application.Features.Quotes.Shared;

namespace Crm.Api.Endpoints.Quotes.CalculateCrmQuoteCustomSewing;

/// <summary>
/// «Виріб з нуля»: prices each fabric with the chosen accessories and operations by the Catalog formula. Nothing is
/// saved; a fabric that cannot be priced lists its problems.
/// </summary>
internal static class CalculateCrmQuoteCustomSewingEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("custom-sewing-price", HandleAsync).WithName("calculateCrmQuoteCustomSewing");

    private static async Task<IResult> HandleAsync(QuoteCustomSewingPriceInput input, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CalculateQuoteCustomSewingQuery(input), cancellationToken)).ToHttpResult();
}
