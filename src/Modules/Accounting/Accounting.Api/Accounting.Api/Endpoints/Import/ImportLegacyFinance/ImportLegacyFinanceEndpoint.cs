using Accounting.Application.Features.Import.Shared.Report;
using Accounting.Application.Features.Import.Commands.ImportLegacyFinance;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Import.ImportLegacyFinance;

internal static class ImportLegacyFinanceEndpoint
{
    /// <summary>Cloud Run accepts 32 MiB per request; the gzipped export is far smaller.</summary>
    private const long MaxRequestBytes = 32 * 1024 * 1024;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("api/accounting/legacy-import", HandleAsync).WithName("importLegacyFinance")
            .WithSummary("Import legacy finance")
            .WithDescription("«Імпорт архіву Фінансів» (stage 5): finance.json of the old Calculator export (format brevi-finance-v5), as is or gzipped by the browser (told by the gzip signature; unpacked up to 200 MiB). dryRun=true only checks and reports. dryRun=false moves the records with their old ids in one transaction and only when the check has no errors; otherwise the report comes back with applied=false and nothing is written. A record whose id Brevi already has is skipped, so a second run adds only what is missing. Old clients and suppliers used in operations are linked to «Клієнти» by KeepinCRM id and to «Постачальники» by KeepinCRM id or a unique name; unknown ones become finance counterparties. Bank statement previews and files are not moved.")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxRequestBytes })
            .Produces<LegacyFinanceImportReport>();

    private static async Task<IResult> HandleAsync(IFormFile? file, [FromServices] ISender sender, bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0) return Results.BadRequest(new { error = "Оберіть файл finance.json." });
        await using var content = file.OpenReadStream();
        return (await sender.Send(new ImportLegacyFinanceCommand(content, dryRun), cancellationToken)).ToHttpResult();
    }
}
