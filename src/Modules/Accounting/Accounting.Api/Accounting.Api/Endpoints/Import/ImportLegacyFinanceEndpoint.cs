using Accounting.Application.Features.Import.Commands.ImportLegacyFinance;

namespace Accounting.Api.Endpoints.Import;

internal static class ImportLegacyFinanceEndpoint
{
    /// <summary>Cloud Run accepts 32 MiB per request; the gzipped export is far smaller.</summary>
    private const long MaxRequestBytes = 32 * 1024 * 1024;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("api/accounting/legacy-import", HandleAsync).WithName("importLegacyFinance")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxRequestBytes });

    private static async Task<IResult> HandleAsync(IFormFile? file, [FromServices] ISender sender, bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0) return Results.BadRequest(new { error = "Оберіть файл finance.json." });
        await using var content = file.OpenReadStream();
        return (await sender.Send(new ImportLegacyFinanceCommand(content, dryRun), cancellationToken)).ToHttpResult();
    }
}
