using Crm.Application.Features.Quotes.Queries.GetQuoteModelComposition;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.GetCrmQuoteModelComposition;

/// <summary>
/// «Змінити склад»: the composition of a Catalog sewing model (the fabric of each variant, shared accessories,
/// services, extra fabrics and operations) to start a changed model from.
/// </summary>
internal static class GetCrmQuoteModelCompositionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("catalog/{productId:int}/composition", HandleAsync).WithName("getCrmQuoteModelComposition")
            .WithSummary("Get CRM quote model composition")
            .WithDescription("«Змінити склад» (R-21): the composition of a Catalog sewing model to start a changed model from: the fabric of each variant with its metres, and the shared accessories, services, extra fabrics and operations. Nothing is saved.")
            .Produces<QuoteModelCompositionResponse>();

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetQuoteModelCompositionQuery(productId), cancellationToken)).ToHttpResult();
}
