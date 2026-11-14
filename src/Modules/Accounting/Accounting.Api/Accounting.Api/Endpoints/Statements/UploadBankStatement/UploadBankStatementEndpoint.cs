using Accounting.Api.Access;
using Accounting.Application.Features.Statements.Commands.UploadBankStatement;

namespace Accounting.Api.Endpoints.Statements.UploadBankStatement;

internal static class UploadBankStatementEndpoint
{
    /// <summary>The file (at most 12 MiB) and a little room for the form around it.</summary>
    private const long MaxRequestBytes = 13 * 1024 * 1024;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("uploadBankStatement")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxRequestBytes });

    /// <param name="accountId">The account the statement belongs to.</param>
    /// <param name="file">The statement as the bank gave it: XLSX, XLS or CSV.</param>
    private static async Task<IResult> HandleAsync([FromForm] Guid accountId, IFormFile? file, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return Results.BadRequest(new { error = "Оберіть файл виписки." });
        await using var content = file.OpenReadStream();
        return (await sender.Send(new UploadBankStatementCommand(accountId, content, file.FileName,
            AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
    }
}
