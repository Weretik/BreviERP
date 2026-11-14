using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Abstractions.Pricing.PpeSupplierPrices.Sources;

namespace Catalog.Api.Endpoints.Ppe.ImportPpeSupplierPrices;

/// <summary>
/// Reads the supplier's price files (gzip allowed) and returns every product of the supplier with its old and new
/// price. With <c>dryRun=false</c> it saves the changed prices; the admin sends the same files again to apply.
/// </summary>
internal static class ImportPpeSupplierPricesEndpoint
{
    /// <summary>Gzipped uploads from the admin are a few MB; the limit stays under Cloud Run's 32 MiB request cap.</summary>
    private const long MaxRequestBytes = 30 * 1024 * 1024;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{key}/import", HandleAsync).WithName("importPpeSupplierPrices")
            .RequireAuthorization("access:price-import")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxRequestBytes });

    private static async Task<IResult> HandleAsync(string key, IFormFileCollection files,
        ClaimsPrincipal user, [FromServices] ISender sender, bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        var actorName = user.Identity?.Name ?? actor;
        var streams = files.Select(file => new SupplierPriceFile(file.FileName, file.OpenReadStream())).ToList();
        try
        {
            return (await sender.Send(
                new ImportPpeSupplierFilesCommand(key, streams, dryRun, actor, actorName),
                    cancellationToken)).ToHttpResult();
        }
        finally
        {
            foreach (var file in streams) await file.Content.DisposeAsync();
        }
    }
}
