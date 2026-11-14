using Accounting.Api.Attachments;
using Accounting.Application.Features.Attachments.Queries.GetFinanceAttachmentContent;

namespace Accounting.Api.Endpoints.Attachments;

/// <summary>The content, shown in the browser: never cached, never sniffed into another type.</summary>
internal static class GetFinanceAttachmentContentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("api/accounting/attachments/{id:guid}/content",
            HandleAsync).WithName("getFinanceAttachmentContent");

    private static async Task<IResult> HandleAsync(Guid id, HttpResponse response, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinanceAttachmentContentQuery(id), cancellationToken);
        if (!result.IsSuccess) return result.ToFileHttpResult();
        response.Headers.CacheControl = "private, no-store";
        response.Headers.XContentTypeOptions = "nosniff";
        response.Headers.ContentDisposition = new System.Net.Mime.ContentDisposition
        {
            Inline = true,
            FileName = result.Value.FileName,
        }.ToString();
        return Results.File(result.Value.Content, result.Value.ContentType);
    }
}
