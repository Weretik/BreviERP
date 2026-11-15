using Accounting.Application.Features.Attachments.Shared;
using Accounting.Application.Features.Attachments.Queries.GetFinanceAttachments;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Attachments.GetFinanceAttachments;

internal static class GetFinanceAttachmentsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("api/accounting/transactions/{id:guid}/attachments",
            HandleAsync).WithName("getFinanceAttachments")
            .WithSummary("Files of one income or expense, oldest first; removed files are left out")
            .Produces<IReadOnlyList<FinanceAttachmentResponse>>();

    private static async Task<IResult> HandleAsync(Guid id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFinanceAttachmentsQuery(id), cancellationToken)).ToHttpResult();
}
