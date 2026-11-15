using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices;
using Purchasing.Application.Features.Register.Shared;
using Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Purchasing.Api.Endpoints.Register.GetPurchaseInvoices;

internal static class GetPurchaseInvoicesEndpoint
{
    public const int DefaultPageSize = 50;

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPurchaseInvoices")
            .WithSummary("One page of cards, one per Telegram message, newest message first (unknown dates last)")
            .WithDescription("One page of cards, one per Telegram message, newest message first (unknown dates last). Search matches the message text (case-insensitive) or an exact UID. Filter WithoutSupplier lists cards with no supplier linked; NotRecognized lists cards with a link that has no checked facts; NeedsReview also includes cards whose last recognition of a file failed.").Produces<PurchaseInvoicePage>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, int page = 1,
        int pageSize = DefaultPageSize, string? search = null, QueryEnum<PurchaseInvoiceFilter>? filter = null,
        int? supplierId = null, DateOnly? documentFrom = null, DateOnly? documentTo = null,
        QueryEnum<PurchaseDocumentVat>? vat = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPurchaseInvoicesQuery(page, pageSize, search,
            filter?.Value ?? PurchaseInvoiceFilter.All, supplierId, documentFrom, documentTo, vat?.Value),
            cancellationToken)).ToHttpResult();
}
