using Accounting.Application.Features.Attachments.Queries.GetFinanceAttachments;

namespace Accounting.Api.Endpoints.Attachments;

internal static class GetFinanceAttachmentsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("api/accounting/transactions/{id:guid}/attachments",
            HandleAsync).WithName("getFinanceAttachments");

    private static async Task<IResult> HandleAsync(Guid id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFinanceAttachmentsQuery(id), cancellationToken)).ToHttpResult();
}
