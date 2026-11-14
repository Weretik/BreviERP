using Accounting.Api.Access;
using Accounting.Application.Features.Attachments.Commands.UploadFinanceAttachment;
using Accounting.Api.Endpoints.Shared;

namespace Accounting.Api.Endpoints.Attachments.UploadFinanceAttachment;

internal static class UploadFinanceAttachmentEndpoint
{
    /// <summary>The file and a little room for the form around it.</summary>
    private const long MaxRequestBytes = 11 * 1024 * 1024;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("api/accounting/transactions/{id:guid}/attachments", HandleAsync)
            .WithName("uploadFinanceAttachment")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxRequestBytes });

    /// <param name="file">A PDF, JPEG or PNG up to 10 MiB.</param>
    private static async Task<IResult> HandleAsync(Guid id, IFormFile? file, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        if (file is null) return Results.BadRequest(new { error = "Оберіть файл." });
        await using var content = file.OpenReadStream();
        return (await sender.Send(new UploadFinanceAttachmentCommand(id, content, file.FileName,
            AccountingAccess.By(user)),
            cancellationToken)).ToFileHttpResult();
    }
}
