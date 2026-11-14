using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Attachments;

/// <summary>
/// Files of an income or expense («Файли» of the operation panel). They live in the private storage and are read only
/// here; 503 while the storage is not configured.
/// </summary>
internal static class FinanceAttachmentsEndpoints
{
    public static IEndpointRouteBuilder MapFinanceAttachmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceAttachments");
        GetFinanceAttachmentsEndpoint.Map(group);
        UploadFinanceAttachmentEndpoint.Map(group);
        DeleteFinanceAttachmentEndpoint.Map(group);
        GetFinanceAttachmentContentEndpoint.Map(group);
        return app;
    }
}
